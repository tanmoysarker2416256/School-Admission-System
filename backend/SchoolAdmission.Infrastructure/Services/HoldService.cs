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
}