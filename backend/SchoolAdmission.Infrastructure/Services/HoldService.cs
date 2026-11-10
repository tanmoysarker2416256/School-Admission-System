using Microsoft.EntityFrameworkCore;
using Npgsql;
using SchoolAdmission.Domain.Entities;
using SchoolAdmission.Domain.Enums;
using SchoolAdmission.Domain.Exceptions;
using SchoolAdmission.Infrastructure.Persistence;

namespace SchoolAdmission.Infrastructure.Services;

public class HoldService(SchoolAdmissionDbContext db, AdmissionSettings settings)
{
    public async Task<Hold> ApplyAsync(Guid classId, string? studentId, CancellationToken ct)
    {
        studentId = studentId?.Trim();
        if (string.IsNullOrEmpty(studentId) || studentId.Length != 17 || !studentId.All(char.IsAsciiDigit))
            throw new BadRequestException("Student ID must be exactly 17 digits.");

        await using var tx = await db.Database.BeginTransactionAsync(ct);
        var now = DateTime.UtcNow;

        // 1. this student's own expired holds stop blocking them
        await db.Holds
            .Where(h => h.StudentId == studentId && h.Status == HoldStatus.Held && h.ExpiresAt <= now)
            .ExecuteUpdateAsync(s => s.SetProperty(h => h.Status, HoldStatus.Expired), ct);

        // 2. one active hold or confirmed seat per student, across all classes
        var hasActive = await db.Holds.AnyAsync(h =>
            h.StudentId == studentId &&
            (h.Status == HoldStatus.Held || h.Status == HoldStatus.Confirmed), ct);
        if (hasActive)
            throw new ConflictException("You already have an active application or a confirmed seat.");

        // 3. lock the class row: other applicants for this class wait here
        var schoolClass = (await db.Classes
                .FromSqlInterpolated($"SELECT * FROM classes WHERE \"Id\" = {classId} FOR UPDATE")
                .AsNoTracking()
                .ToListAsync(ct))
            .SingleOrDefault();
        if (schoolClass is null)
            throw new NotFoundException("Class not found.");

        // 4. count seats while holding the lock
        var held = await db.Holds.CountAsync(h =>
            h.ClassId == classId && h.Status == HoldStatus.Held && h.ExpiresAt > now, ct);
        var confirmed = await db.Holds.CountAsync(h =>
            h.ClassId == classId && h.Status == HoldStatus.Confirmed, ct);

        if (held + confirmed >= schoolClass.TotalSeats)
            throw new ConflictException("No seats are available in this class right now.");

        // 5. create the hold
        var hold = new Hold
        {
            Id = Guid.NewGuid(),
            StudentId = studentId,
            ClassId = classId,
            Status = HoldStatus.Held,
            CreatedAt = now,
            ExpiresAt = now.AddSeconds(settings.HoldDurationSeconds)
        };
        db.Holds.Add(hold);

        try
        {
            await db.SaveChangesAsync(ct);
        }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation })
        {
            // the same student applied twice at the same moment; the unique index stopped the second one
            throw new ConflictException("You already have an active application or a confirmed seat.");
        }

        await tx.CommitAsync(ct);
        return hold;
    }


    public async Task<Hold> GetAsync(Guid holdId, CancellationToken ct) =>
    await db.Holds.AsNoTracking().FirstOrDefaultAsync(h => h.Id == holdId, ct)
        ?? throw new NotFoundException("Application not found.");

    public async Task<Hold> ConfirmAsync(Guid holdId, string? studentId, CancellationToken ct)
    {
        studentId = StudentIdRules.Normalize(studentId);

        await using var tx = await db.Database.BeginTransactionAsync(ct);
        var now = DateTime.UtcNow;
        var hold = await LockHoldAsync(holdId, ct);

        if (hold.StudentId != studentId)
            throw new ForbiddenException("This application belongs to another student.");

        switch (hold.EffectiveStatus(now))
        {
            case HoldStatus.Confirmed:
                return hold;   // confirming again is fine; the seat is counted only once
            case HoldStatus.Cancelled:
                throw new ConflictException("This application was cancelled.");
            case HoldStatus.Expired:
                throw new ConflictException("This application has expired. Please apply again.");
        }

        hold.Status = HoldStatus.Confirmed;
        hold.ConfirmedAt = now;
        await db.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);
        return hold;
    }

    public async Task CancelAsync(Guid holdId, string? studentId, CancellationToken ct)
    {
        studentId = StudentIdRules.Normalize(studentId);

        await using var tx = await db.Database.BeginTransactionAsync(ct);
        var now = DateTime.UtcNow;
        var hold = await LockHoldAsync(holdId, ct);

        if (hold.StudentId != studentId)
            throw new ForbiddenException("This application belongs to another student.");

        switch (hold.EffectiveStatus(now))
        {
            case HoldStatus.Confirmed:
                throw new ConflictException("A confirmed application cannot be cancelled.");
            case HoldStatus.Cancelled:
            case HoldStatus.Expired:
                return;   // nothing left to cancel
        }

        hold.Status = HoldStatus.Cancelled;
        hold.CancelledAt = now;
        await db.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);
    }

    public async Task<List<MyHoldSummary>> GetMineAsync(string? studentId, CancellationToken ct)
    {
        studentId = StudentIdRules.Normalize(studentId);
        var now = DateTime.UtcNow;

        var rows = await db.Holds.AsNoTracking()
            .Where(h => h.StudentId == studentId)
            .OrderByDescending(h => h.CreatedAt)
            .Select(h => new { h.Id, h.ClassId, ClassName = h.SchoolClass!.Name, h.Status, h.ExpiresAt })
            .ToListAsync(ct);

        return rows
            .Select(r => new MyHoldSummary(
                r.Id, r.ClassId, r.ClassName,
                r.Status == HoldStatus.Held && r.ExpiresAt <= now ? HoldStatus.Expired : r.Status,
                r.ExpiresAt))
            .ToList();
    }

    // locks the hold row so concurrent confirm/cancel calls for it run one at a time
    private async Task<Hold> LockHoldAsync(Guid holdId, CancellationToken ct)
    {
        var hold = (await db.Holds
                .FromSqlInterpolated($"SELECT * FROM holds WHERE \"Id\" = {holdId} FOR UPDATE")
                .ToListAsync(ct))
            .SingleOrDefault();

        return hold ?? throw new NotFoundException("Application not found.");
    }
}