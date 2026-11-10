using SchoolAdmission.Domain.Exceptions;

namespace SchoolAdmission.Tests;

[Collection("db")]
public class ConcurrencyTests(PostgresFixture fixture) : AdmissionTestBase(fixture)
{
    [Fact]
    public async Task Many_simultaneous_applications_never_exceed_total_seats()
    {
        const int seats = 5;
        const int applicants = 40;
        var cls = await CreateClassAsync(seats);

        var gate = new TaskCompletionSource();
        var tasks = Enumerable.Range(0, applicants).Select(_ => Task.Run(async () =>
        {
            await gate.Task;   // everyone waits here until the gate opens
            try
            {
                await ApplyAsync(cls.Id, NewStudentId());
                return true;
            }
            catch (ConflictException)
            {
                return false;
            }
        })).ToList();

        gate.SetResult();      // all 40 start at the same moment
        var results = await Task.WhenAll(tasks);

        Assert.Equal(seats, results.Count(won => won));
        Assert.Equal(applicants - seats, results.Count(won => !won));

        var s = await SummaryAsync(cls.Id);
        Assert.Equal(seats, s.HeldSeats);
        Assert.Equal(0, s.AvailableSeats);
        Assert.True(s.HeldSeats + s.ConfirmedSeats <= s.TotalSeats);
    }

    [Fact]
    public async Task Same_student_applying_many_times_at_once_gets_only_one_seat()
    {
        var classes = await Task.WhenAll(Enumerable.Range(0, 5).Select(_ => CreateClassAsync(10)));
        var student = NewStudentId();

        var gate = new TaskCompletionSource();
        var tasks = Enumerable.Range(0, 10).Select(i => Task.Run(async () =>
        {
            await gate.Task;
            try
            {
                await ApplyAsync(classes[i % classes.Length].Id, student);
                return true;
            }
            catch (ConflictException)
            {
                return false;
            }
        })).ToList();

        gate.SetResult();
        var results = await Task.WhenAll(tasks);

        Assert.Equal(1, results.Count(won => won));

        var totalHeld = 0;
        foreach (var c in classes)
            totalHeld += (await SummaryAsync(c.Id)).HeldSeats;
        Assert.Equal(1, totalHeld);
    }

    [Fact]
    public async Task Confirming_the_same_hold_many_times_at_once_counts_one_seat()
    {
        var cls = await CreateClassAsync(10);
        var student = NewStudentId();
        var hold = await ApplyAsync(cls.Id, student);

        var gate = new TaskCompletionSource();
        var tasks = Enumerable.Range(0, 10).Select(_ => Task.Run(async () =>
        {
            await gate.Task;
            await ConfirmAsync(hold.Id, student);
        })).ToList();

        gate.SetResult();
        await Task.WhenAll(tasks);   // every call succeeds; confirming again is allowed

        var s = await SummaryAsync(cls.Id);
        Assert.Equal(1, s.ConfirmedSeats);
        Assert.Equal(0, s.HeldSeats);
    }
}