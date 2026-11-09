namespace SchoolAdmission.Api.Contracts;

public record CreateClassRequest(string? Name, int TotalSeats);

public record ClassResponse(Guid Id, string Name, int TotalSeats);