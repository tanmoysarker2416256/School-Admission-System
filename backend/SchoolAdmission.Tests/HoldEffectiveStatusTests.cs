using SchoolAdmission.Domain.Entities;
using SchoolAdmission.Domain.Enums;

namespace SchoolAdmission.Tests;

public class HoldEffectiveStatusTests
{
    private static readonly DateTime Now = new(2026, 11, 10, 12, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void Held_before_deadline_stays_held()
    {
        var hold = new Hold { Status = HoldStatus.Held, ExpiresAt = Now.AddSeconds(30) };
        Assert.Equal(HoldStatus.Held, hold.EffectiveStatus(Now));
    }

    [Fact]
    public void Held_after_deadline_is_expired()
    {
        var hold = new Hold { Status = HoldStatus.Held, ExpiresAt = Now.AddSeconds(-1) };
        Assert.Equal(HoldStatus.Expired, hold.EffectiveStatus(Now));
    }

    [Fact]
    public void Confirmed_hold_never_turns_expired()
    {
        var hold = new Hold { Status = HoldStatus.Confirmed, ExpiresAt = Now.AddHours(-1) };
        Assert.Equal(HoldStatus.Confirmed, hold.EffectiveStatus(Now));
    }
}