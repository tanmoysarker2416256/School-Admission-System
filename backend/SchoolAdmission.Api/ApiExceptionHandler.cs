using Microsoft.AspNetCore.Diagnostics;
using SchoolAdmission.Domain.Exceptions;

namespace SchoolAdmission.Api;

public class ApiExceptionHandler : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext context, Exception exception, CancellationToken ct)
    {
        var status = exception switch
        {
            BadRequestException => StatusCodes.Status400BadRequest,
            NotFoundException => StatusCodes.Status404NotFound,
            ConflictException => StatusCodes.Status409Conflict,
            _ => 0
        };

        if (status == 0) return false;   // anything else stays a normal 500

        context.Response.StatusCode = status;
        await context.Response.WriteAsJsonAsync(new { error = exception.Message }, ct);
        return true;
    }
}