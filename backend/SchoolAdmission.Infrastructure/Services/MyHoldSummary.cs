using SchoolAdmission.Domain.Enums;

namespace SchoolAdmission.Infrastructure.Services;

public record MyHoldSummary(Guid HoldId, Guid ClassId, string ClassName, HoldStatus Status, DateTime ExpiresAt);