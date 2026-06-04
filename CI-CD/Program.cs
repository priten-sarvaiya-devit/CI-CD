using CI_CD.Services;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// ---------------------------------------------------------------------------
// Service registration (Dependency Injection container)
// ---------------------------------------------------------------------------
builder.Services.AddControllers();

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// Application services.
builder.Services.AddScoped<IWeatherService, WeatherService>();

var app = builder.Build();

// ---------------------------------------------------------------------------
// HTTP request pipeline
// ---------------------------------------------------------------------------
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

// Lightweight liveness endpoint (useful for Render/uptime health checks).
app.MapGet("/health", () => Results.Ok(new { status = "Healthy" }));

app.Run();

// Exposed so the integration/unit test project can reference the entry point.
public partial class Program { }
