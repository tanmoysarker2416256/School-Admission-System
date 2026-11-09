using Microsoft.EntityFrameworkCore;
using Npgsql;
using SchoolAdmission.Domain.Entities;
using SchoolAdmission.Domain.Enums;
using SchoolAdmission.Domain.Exceptions;
using SchoolAdmission.Infrastructure.Persistence;

namespace SchoolAdmission.Infrastructure.Services;

public class ClassService(SchoolAdmissionDbContext db)
{
    public async Task<SchoolClass> CreateAsync(string? name, int totalSeats, CancellationToken ct)
    {
        name = name?.Trim();

        if (string.IsNullOrEmpty(name))
            throw new BadRequestException("Class name is required.");
        if (name.Length > 100)
            throw new BadRequestException("Class name must be 100 characters or fewer.");
        if (totalSeats <= 0)
            throw new BadRequestException("Total seats must be greater than zero.");

        if (await db.Classes.AnyAsync(c => c.Name == name, ct))
            throw new ConflictException("A class with this name already exists.");

        var schoolClass = new SchoolClass
        {
            Id = Guid.NewGuid(),
            Name = name,
            TotalSeats = totalSeats,
            CreatedAt = DateTime.UtcNow
        };

        db.Classes.Add(schoolClass);

        try
        {
            await db.SaveChangesAsync(ct);
        }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation })
        {
            // two admins created the same name at the same moment; the database unique index caught it
            throw new ConflictException("A class with this name already exists.");
        }

        return schoolClass;
    }

    public async Task<List<ClassSeatSummary>> GetAllAsync(CancellationToken ct)
    {
        var now = DateTime.UtcNow;
        var classes = db.Classes.AsNoTracking().OrderBy(c => c.CreatedAt);
        return await ToSummaries(classes, now).ToListAsync(ct);
    }

    public async Task<ClassSeatSummary> GetByIdAsync(Guid id, CancellationToken ct)
    {
        var now = DateTime.UtcNow;
        var classes = db.Classes.AsNoTracking().Where(c => c.Id == id);
        var summary = await ToSummaries(classes, now).FirstOrDefaultAsync(ct);

        return summary ?? throw new NotFoundException("Class not found.");
    }

    private IQueryable<ClassSeatSummary> ToSummaries(IQueryable<SchoolClass> classes, DateTime now) =>
        classes.Select(c => new ClassSeatSummary(
            c.Id,
            c.Name,
            c.TotalSeats,
            db.Holds.Count(h => h.ClassId == c.Id && h.Status == HoldStatus.Held && h.ExpiresAt > now),
            db.Holds.Count(h => h.ClassId == c.Id && h.Status == HoldStatus.Confirmed)));
}