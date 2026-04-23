using Ocelot.Cache.CacheManager;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using Ocelot.Provider.Eureka;
using Ocelot.Provider.Polly;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Load ocelot.json on top of the default config
IConfiguration ocelotConfig = new ConfigurationBuilder()
    .AddJsonFile("ocelot.json")
    .Build();

// Ocelot routes + Eureka service discovery + Polly resilience
builder.Services.AddOcelot(ocelotConfig)
    .AddEureka()
    .AddPolly();

// OpenAPI docs (built-in .NET 10 — replaces Swashbuckle)
// UI available at: /scalar/v1
builder.Services.AddOpenApi();

builder.Services.AddCors();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();            // serves spec at /openapi/v1.json
    app.MapScalarApiReference(); // serves UI at /scalar/v1
}

app.UseHttpsRedirection();
app.UseCors(b => b.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());

// Ocelot middleware must be last — it handles all routing
await app.UseOcelot();

app.Run();