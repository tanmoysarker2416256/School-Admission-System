using Microsoft.AspNetCore.Mvc;
using SchoolAdmission.Api.Contracts;
using SchoolAdmission.Infrastructure.Services;

namespace SchoolAdmission.Api.Endpoints;

public static class HoldEndpoints
{
    public static void MapHoldEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost("/classes/{classId:guid}/holds", async (
            Guid classId,
            [FromHeader(Name = "X-Student-Id")] string? studentId,
            HoldService service,
            CancellationToken ct) =>
        {
            var hold = await service.ApplyAsync(classId, studentId, ct);
            return Results.Created($"/holds/{hold.Id}", HoldCreatedResponse.From(hold));
        });
    }
}