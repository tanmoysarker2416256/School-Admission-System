using SchoolAdmission.Infrastructure.Services;

namespace SchoolAdmission.Api.Contracts;

public record CreateClassRequest(string? Name, int TotalSeats);

public record ClassResponse(Guid Id, string Name, int TotalSeats);

public record ClassSeatsResponse(
    Guid Id,
    string Name,
    int TotalSeats,
    int HeldSeats,
    int ConfirmedSeats,
    int AvailableSeats)
{
    public static ClassSeatsResponse From(ClassSeatSummary s) =>
        new(s.Id, s.Name, s.TotalSeats, s.HeldSeats, s.ConfirmedSeats, s.AvailableSeats);
}