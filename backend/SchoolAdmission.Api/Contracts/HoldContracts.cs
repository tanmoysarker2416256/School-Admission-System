using SchoolAdmission.Domain.Entities;
using SchoolAdmission.Domain.Enums;
using SchoolAdmission.Infrastructure.Services;

namespace SchoolAdmission.Api.Contracts;

public record HoldCreatedResponse(Guid HoldId, Guid ClassId, string StudentId, string Status, DateTime ExpiresAt)
{
    public static HoldCreatedResponse From(Hold h) =>
        new(h.Id, h.ClassId, h.StudentId, h.Status.ToApiString(), h.ExpiresAt);
}

public static class HoldStatusExtensions
{
    // the brief shows HELD, CONFIRMED, EXPIRED, CANCELLED
    public static string ToApiString(this HoldStatus status) => status.ToString().ToUpperInvariant();
}

public record HoldStatusResponse(Guid HoldId, Guid ClassId, string Status, DateTime ExpiresAt)
{
    public static HoldStatusResponse From(Hold h, DateTime now) =>
        new(h.Id, h.ClassId, h.EffectiveStatus(now).ToApiString(), h.ExpiresAt);
}

public record HoldConfirmedResponse(Guid HoldId, string Status);

public record MyHoldResponse(Guid HoldId, Guid ClassId, string ClassName, string Status, DateTime ExpiresAt)
{
    public static MyHoldResponse From(MyHoldSummary s) =>
        new(s.HoldId, s.ClassId, s.ClassName, s.Status.ToApiString(), s.ExpiresAt);
}