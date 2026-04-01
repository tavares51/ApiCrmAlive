using System.Net.Mime;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ApiCrmAlive.Middleware;

/// <summary>
/// Minimal exception-to-HTTP mapping for the API.
/// Keeps existing controllers/services simple (they throw) while returning meaningful status codes.
/// </summary>
public sealed class ApiExceptionMiddleware(
    RequestDelegate next,
    ILogger<ApiExceptionMiddleware> logger,
    IHostEnvironment env)
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public async Task Invoke(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            var (status, title, detail) = MapToProblem(context, ex, env);

            if (status >= StatusCodes.Status500InternalServerError)
            {
                logger.LogError(ex, "Unhandled exception for {Method} {Path}", context.Request.Method, context.Request.Path);
            }
            else
            {
                // Expected/handled exceptions (4xx/409) are still useful to track, but shouldn't pollute logs as failures.
                logger.LogWarning("Handled exception for {Method} {Path}. Status {Status}. Detail: {Detail}",
                    context.Request.Method, context.Request.Path, status, detail);
            }

            await WriteProblemDetails(context, status, title, detail);
        }
    }

    private static (int status, string title, string detail) MapToProblem(HttpContext ctx, Exception ex, IHostEnvironment env)
    {
        var (status, title) = ex switch
        {
            KeyNotFoundException => (StatusCodes.Status404NotFound, "Not Found"),
            ArgumentException => (StatusCodes.Status400BadRequest, "Bad Request"),
            InvalidOperationException => (StatusCodes.Status409Conflict, "Conflict"),
            DbUpdateException => (StatusCodes.Status409Conflict, "Conflict"),
            UnauthorizedAccessException => (StatusCodes.Status403Forbidden, "Forbidden"),
            _ => (StatusCodes.Status500InternalServerError, "Internal Server Error")
        };

        // EF Core often wraps the actionable database error in InnerException. Surface it in Development
        // to make debugging possible without reading server logs.
        var detail = ex.Message;
        if (env.IsDevelopment() && ex is DbUpdateException && ex.InnerException is not null)
        {
            detail = $"{ex.Message} InnerException: {ex.InnerException.Message}";
        }

        return (status, title, detail);
    }

    private static async Task WriteProblemDetails(HttpContext ctx, int status, string title, string detail)
    {
        if (ctx.Response.HasStarted)
            return;

        var problem = new ProblemDetails
        {
            Status = status,
            Title = title,
            Detail = detail,
            Instance = ctx.Request.Path
        };

        ctx.Response.Clear();
        ctx.Response.StatusCode = status;
        ctx.Response.ContentType = MediaTypeNames.Application.Json;

        await ctx.Response.WriteAsync(JsonSerializer.Serialize(problem, JsonOptions));
    }
}
