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
    }
}