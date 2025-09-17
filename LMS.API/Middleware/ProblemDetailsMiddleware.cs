using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using System.Net;
using System.Text.Json;

namespace LMS.API.Middleware;
public class ProblemDetailsMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ProblemDetailsMiddleware> _logger;
    public ProblemDetailsMiddleware(RequestDelegate next, ILogger<ProblemDetailsMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Business rule violation");
            await WriteProblemAsync(context, StatusCodes.Status409Conflict, ex.Message, "https://datatracker.ietf.org/doc/html/rfc7231#section-6.5.8");
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Unauthorized access");
            await WriteProblemAsync(context, StatusCodes.Status401Unauthorized, ex.Message, "https://datatracker.ietf.org/doc/html/rfc7235#section-3.1");
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "Entity not found");
            await WriteProblemAsync(context, StatusCodes.Status404NotFound, ex.Message, "https://datatracker.ietf.org/doc/html/rfc7231#section-6.5.4");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception");
            await WriteProblemAsync(context, StatusCodes.Status500InternalServerError, "Ett oväntat fel inträffade", "https://datatracker.ietf.org/doc/html/rfc7231#section-6.6.1");
        }
    }
    private static async Task WriteProblemAsync(HttpContext context, int statusCode, string detail, string type)
    {
        if (context.Response.HasStarted)
            return;
        var problem = new ProblemDetails
        {
            Status = statusCode,
            Title = ReasonPhrases.GetReasonPhrase(statusCode),
            Detail = detail,
            Type = type,
            Instance = context.Request.Path
        };
        context.Response.ContentType = "application/problem+json";
        context.Response.StatusCode = statusCode;
        var json = JsonSerializer.Serialize(problem, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
        await context.Response.WriteAsync(json);
    }
}
public static class ProblemDetailsMiddlewareExtensions
{
    public static IApplicationBuilder UseStandardProblemDetails(this IApplicationBuilder app)
=> app.UseMiddleware<ProblemDetailsMiddleware>();
}
