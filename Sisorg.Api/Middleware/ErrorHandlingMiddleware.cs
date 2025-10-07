using System.Text.Json;
using Sisorg.Api.ViewModels;

namespace Sisorg.Api.Middleware;

public class ErrorHandlingMiddleware
{
    private readonly RequestDelegate _next;
    public ErrorHandlingMiddleware(RequestDelegate next) => _next = next;

    public async Task Invoke(HttpContext ctx)
    {
        var correlationId = Guid.NewGuid().ToString();
        try
        {
            await _next(ctx);
        }
        catch (InvalidOperationException vex)
        {
            await Write(ctx, 400, "VALIDATION_ERROR", vex.Message, correlationId);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"[{correlationId}] {ex}");
            await Write(ctx, 500, "INTERNAL_ERROR", "An unexpected error occurred.", correlationId);
        }
    }

    private static async Task Write(HttpContext ctx, int status, string code, string message, string correlationId)
    {
        ctx.Response.StatusCode = status;
        ctx.Response.ContentType = "application/json";
        var payload = new ErrorViewModel { Code = code, Message = message, CorrelationId = correlationId };
        await ctx.Response.WriteAsync(JsonSerializer.Serialize(payload));
    }
}
