using Microsoft.AspNetCore.Mvc;
using UserManagementAPI.Middleware;
using UserManagementAPI.Repositories;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.InvalidModelStateResponseFactory = context =>
    {
        var errors = context.ModelState
            .Where(x => x.Value?.Errors.Count > 0)
            .ToDictionary(
                x => x.Key,
                x => x.Value!.Errors
                    .Select(e => string.IsNullOrWhiteSpace(e.ErrorMessage) ? "Invalid value." : e.ErrorMessage)
                    .ToArray()
            );

        return new BadRequestObjectResult(new
        {
            error = "Validation failed.",
            details = errors
        });
    };
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSingleton<IUserRepository, InMemoryUserRepository>();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

// Ordem pedida pela atividade:
// 1) Error handling
// 2) Authentication
// 3) Logging
app.UseMiddleware<ErrorHandlingMiddleware>();
app.UseMiddleware<TokenAuthenticationMiddleware>();
app.UseMiddleware<RequestResponseLoggingMiddleware>();

app.MapGet("/", () => Results.Ok(new
{
    message = "UserManagementAPI is running."
}));

app.MapControllers();

app.Run();