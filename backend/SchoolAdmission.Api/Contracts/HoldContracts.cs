using SchoolAdmission.Domain.Entities;
using SchoolAdmission.Domain.Enums;

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