using System.Text.Json;

namespace UserManagementAPI.Middleware;

public class TokenAuthenticationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IConfiguration _configuration;

    public TokenAuthenticationMiddleware(RequestDelegate next, IConfiguration configuration)
    {
        _next = next;
        _configuration = configuration;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var path = context.Request.Path.Value ?? string.Empty;

        // Permitir acesso ao Swagger e rota raiz sem token
        if (path.StartsWith("/swagger", StringComparison.OrdinalIgnoreCase) ||
            path.Equals("/", StringComparison.OrdinalIgnoreCase) ||
            path.StartsWith("/favicon", StringComparison.OrdinalIgnoreCase))
        {
            await _next(context);
            return;
        }

        var expectedToken = _configuration["Auth:ApiToken"];

        if (!context.Request.Headers.TryGetValue("Authorization", out var authHeader))
        {
            await WriteUnauthorizedAsync(context);
            return;
        }

        var headerValue = authHeader.ToString();

        if (!headerValue.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
        {
            await WriteUnauthorizedAsync(context);
            return;
        }

        var providedToken = headerValue["Bearer ".Length..].Trim();

        if (string.IsNullOrWhiteSpace(expectedToken) || !string.Equals(providedToken, expectedToken, StringComparison.Ordinal))
        {
            await WriteUnauthorizedAsync(context);
            return;
        }

        await _next(context);
    }

    private static async Task WriteUnauthorizedAsync(HttpContext context)
    {
        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
        context.Response.ContentType = "application/json";

        var response = new
        {
            error = "Unauthorized. Invalid or missing token."
        };

        await context.Response.WriteAsync(JsonSerializer.Serialize(response));
    }
}