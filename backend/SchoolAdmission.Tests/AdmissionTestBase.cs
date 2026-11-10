using Microsoft.EntityFrameworkCore;
using SchoolAdmission.Domain.Entities;
using SchoolAdmission.Infrastructure;
using SchoolAdmission.Infrastructure.Services;

namespace SchoolAdmission.Tests;

public abstract class AdmissionTestBase(PostgresFixture fixture)
{
    private static readonly AdmissionSettings Settings = new(600);

    protected async Task<SchoolClass> CreateClassAsync(int seats)
    {
        await using var db = fixture.CreateDb();
        return await new ClassService(db).CreateAsync($"Test {Guid.NewGuid()}", seats, CancellationToken.None);
    }

    // every helper uses its own DbContext, like separate web requests
    protected async Task<Hold> ApplyAsync(Guid classId, string studentId)
    {
        await using var db = fixture.CreateDb();
        return await new HoldService(db, Settings).ApplyAsync(classId, studentId, CancellationToken.None);
    }

    protected async Task<Hold> ConfirmAsync(Guid holdId, string studentId)
    {
        await using var db = fixture.CreateDb();
        return await new HoldService(db, Settings).ConfirmAsync(holdId, studentId, CancellationToken.None);
    }

    protected async Task CancelAsync(Guid holdId, string studentId)
    {
        await using var db = fixture.CreateDb();
        await new HoldService(db, Settings).CancelAsync(holdId, studentId, CancellationToken.None);
    }

    protected async Task<ClassSeatSummary> SummaryAsync(Guid classId)
    {
        await using var db = fixture.CreateDb();
        return await new ClassService(db).GetByIdAsync(classId, CancellationToken.None);
    }

    // simulates "10 minutes passed" without waiting
    protected async Task ForceExpireAsync(Guid holdId)
    {
        await using var db = fixture.CreateDb();
        await db.Holds.Where(h => h.Id == holdId)
            .ExecuteUpdateAsync(s => s.SetProperty(h => h.ExpiresAt, DateTime.UtcNow.AddMinutes(-1)));
    }

    protected static string NewStudentId() =>
        Random.Shared.NextInt64(10_000_000_000_000_000, 100_000_000_000_000_000).ToString();   // always 17 digits
}