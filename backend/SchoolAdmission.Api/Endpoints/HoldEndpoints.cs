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


        app.MapGet("/holds/{holdId:guid}", async (Guid holdId, HoldService service, CancellationToken ct) =>
        {
            var hold = await service.GetAsync(holdId, ct);
            return Results.Ok(HoldStatusResponse.From(hold, DateTime.UtcNow));
        });

        app.MapPost("/holds/{holdId:guid}/confirm", async (
            Guid holdId,
            [FromHeader(Name = "X-Student-Id")] string? studentId,
            HoldService service,
            CancellationToken ct) =>
        {
            var hold = await service.ConfirmAsync(holdId, studentId, ct);
            return Results.Ok(new HoldConfirmedResponse(hold.Id, hold.Status.ToApiString()));
        });

        app.MapDelete("/holds/{holdId:guid}", async (
            Guid holdId,
            [FromHeader(Name = "X-Student-Id")] string? studentId,
            HoldService service,
            CancellationToken ct) =>
        {
            await service.CancelAsync(holdId, studentId, ct);
            return Results.NoContent();
        });

        app.MapGet("/me/holds", async (
            [FromHeader(Name = "X-Student-Id")] string? studentId,
            HoldService service,
            CancellationToken ct) =>
        {
            var items = await service.GetMineAsync(studentId, ct);
            return Results.Ok(items.Select(MyHoldResponse.From));
        });
    }
}