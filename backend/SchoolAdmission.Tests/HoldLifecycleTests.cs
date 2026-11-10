using SchoolAdmission.Domain.Exceptions;

namespace SchoolAdmission.Tests;

[Collection("db")]
public class HoldLifecycleTests(PostgresFixture fixture) : AdmissionTestBase(fixture)
{
    [Fact]
    public async Task Apply_holds_one_seat()
    {
        var cls = await CreateClassAsync(10);

        await ApplyAsync(cls.Id, NewStudentId());

        var s = await SummaryAsync(cls.Id);
        Assert.Equal(1, s.HeldSeats);
        Assert.Equal(0, s.ConfirmedSeats);
        Assert.Equal(9, s.AvailableSeats);
    }

    [Theory]
    [InlineData("")]
    [InlineData("1234567890123456")]      // 16 digits
    [InlineData("123456789012345678")]    // 18 digits
    [InlineData("1234567890123456a")]     // contains a letter
    public async Task Apply_rejects_invalid_student_id(string studentId)
    {
        var cls = await CreateClassAsync(10);

        await Assert.ThrowsAsync<BadRequestException>(() => ApplyAsync(cls.Id, studentId));
    }

    [Fact]
    public async Task Apply_to_full_class_conflicts()
    {
        var cls = await CreateClassAsync(1);
        await ApplyAsync(cls.Id, NewStudentId());

        await Assert.ThrowsAsync<ConflictException>(() => ApplyAsync(cls.Id, NewStudentId()));
    }

    [Fact]
    public async Task Student_cannot_hold_two_classes()
    {
        var first = await CreateClassAsync(5);
        var second = await CreateClassAsync(5);
        var student = NewStudentId();
        await ApplyAsync(first.Id, student);

        await Assert.ThrowsAsync<ConflictException>(() => ApplyAsync(second.Id, student));
    }

    [Fact]
    public async Task Expired_hold_frees_the_seat()
    {
        var cls = await CreateClassAsync(1);
        var hold = await ApplyAsync(cls.Id, NewStudentId());

        await ForceExpireAsync(hold.Id);
        await ApplyAsync(cls.Id, NewStudentId());   // would throw 409 if the seat were still blocked

        var s = await SummaryAsync(cls.Id);
        Assert.Equal(1, s.HeldSeats);
    }

    [Fact]
    public async Task Student_can_apply_again_after_own_hold_expired()
    {
        var cls = await CreateClassAsync(5);
        var student = NewStudentId();
        var hold = await ApplyAsync(cls.Id, student);

        await ForceExpireAsync(hold.Id);

        await ApplyAsync(cls.Id, student);   // the old row must not trip the unique index
    }

    [Fact]
    public async Task Confirm_moves_seat_from_held_to_confirmed()
    {
        var cls = await CreateClassAsync(10);
        var student = NewStudentId();
        var hold = await ApplyAsync(cls.Id, student);

        await ConfirmAsync(hold.Id, student);

        var s = await SummaryAsync(cls.Id);
        Assert.Equal(0, s.HeldSeats);
        Assert.Equal(1, s.ConfirmedSeats);
        Assert.Equal(9, s.AvailableSeats);
    }

    [Fact]
    public async Task Confirming_twice_counts_the_seat_once()
    {
        var cls = await CreateClassAsync(10);
        var student = NewStudentId();
        var hold = await ApplyAsync(cls.Id, student);

        await ConfirmAsync(hold.Id, student);
        await ConfirmAsync(hold.Id, student);

        var s = await SummaryAsync(cls.Id);
        Assert.Equal(1, s.ConfirmedSeats);
    }

    [Fact]
    public async Task Other_student_cannot_confirm()
    {
        var cls = await CreateClassAsync(10);
        var hold = await ApplyAsync(cls.Id, NewStudentId());

        await Assert.ThrowsAsync<ForbiddenException>(() => ConfirmAsync(hold.Id, NewStudentId()));
    }

    [Fact]
    public async Task Expired_hold_cannot_be_confirmed()
    {
        var cls = await CreateClassAsync(10);
        var student = NewStudentId();
        var hold = await ApplyAsync(cls.Id, student);

        await ForceExpireAsync(hold.Id);

        await Assert.ThrowsAsync<ConflictException>(() => ConfirmAsync(hold.Id, student));
    }

    [Fact]
    public async Task Cancel_frees_the_seat_and_student_can_apply_again()
    {
        var cls = await CreateClassAsync(1);
        var student = NewStudentId();
        var hold = await ApplyAsync(cls.Id, student);

        await CancelAsync(hold.Id, student);

        var s = await SummaryAsync(cls.Id);
        Assert.Equal(0, s.HeldSeats);
        await ApplyAsync(cls.Id, student);
    }

    [Fact]
    public async Task Confirmed_hold_cannot_be_cancelled()
    {
        var cls = await CreateClassAsync(10);
        var student = NewStudentId();
        var hold = await ApplyAsync(cls.Id, student);
        await ConfirmAsync(hold.Id, student);

        await Assert.ThrowsAsync<ConflictException>(() => CancelAsync(hold.Id, student));
    }
}