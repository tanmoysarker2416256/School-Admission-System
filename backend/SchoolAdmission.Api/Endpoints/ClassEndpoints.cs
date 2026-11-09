using SchoolAdmission.Api.Contracts;
using SchoolAdmission.Infrastructure.Services;

namespace SchoolAdmission.Api.Endpoints;

public static class ClassEndpoints
{
    public static void MapClassEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost("/classes", async (CreateClassRequest request, ClassService service, CancellationToken ct) =>
        {
            var created = await service.CreateAsync(request.Name, request.TotalSeats, ct);
            return Results.Created($"/classes/{created.Id}",
                new ClassResponse(created.Id, created.Name, created.TotalSeats));
        });

        app.MapGet("/classes", async (ClassService service, CancellationToken ct) =>
        {
            var items = await service.GetAllAsync(ct);
            return Results.Ok(items.Select(ClassSeatsResponse.From));
        });

        app.MapGet("/classes/{classId:guid}", async (Guid classId, ClassService service, CancellationToken ct) =>
        {
            var item = await service.GetByIdAsync(classId, ct);
            return Results.Ok(ClassSeatsResponse.From(item));
        });
    }
}