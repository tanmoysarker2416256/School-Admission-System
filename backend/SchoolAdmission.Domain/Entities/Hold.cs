using SchoolAdmission.Domain.Enums;

namespace SchoolAdmission.Domain.Entities;

public class Hold
{
    public Guid Id { get; set; }
    public string StudentId { get; set; } = string.Empty;
    public Guid ClassId { get; set; }
    public HoldStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime ExpiresAt { get; set; }
    public DateTime? ConfirmedAt { get; set; }
    public DateTime? CancelledAt { get; set; }

    public SchoolClass? SchoolClass { get; set; }

    // A hold past its deadline counts as expired even if nobody has updated the row yet.
    public HoldStatus EffectiveStatus(DateTime now) =>
        Status == HoldStatus.Held && ExpiresAt <= now ? HoldStatus.Expired : Status;
}