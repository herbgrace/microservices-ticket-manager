using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

// eureka
using Steeltoe.Discovery.Client;
using Steeltoe.Discovery.Eureka;

using Pomelo.EntityFrameworkCore.MySql.Infrastructure;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// OpenAPI docs (built-in .NET 10 — replaces Swashbuckle)
// UI available at: /scalar/v1
builder.Services.AddOpenApi();

// EF Core
builder.Services.AddDbContext<OrderServiceDbContext>(options =>
    options.UseMySql(
        builder.Configuration.GetConnectionString("conn_orderservice_mysql"),
        new MySqlServerVersion(new Version(8, 0, 33)),
        mySqlOptions => mySqlOptions.EnableRetryOnFailure(5, TimeSpan.FromSeconds(10), null)));

// Controllers + JSON serialization
builder.Services.AddControllers().AddNewtonsoftJson(options =>
    options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore);

// AutoMapper
builder.Services.AddAutoMapper(typeof(Program));

// JWT Auth
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(o =>
{
    o.TokenValidationParameters = new TokenValidationParameters
    {
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"] ?? throw new InvalidOperationException("Jwt:Key is not configured."))),
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true, // Enable this in production!
        ClockSkew = TimeSpan.Zero,
        ValidateIssuerSigningKey = true
    };
});
builder.Services.AddAuthorization();

// Eureka / Steeltoe Service Discovery (currently disabled)
// To re-enable: uncomment the line below and the UseDiscoveryClient() call at the bottom.
// NOTE: Steeltoe 3.2.8 (in the .csproj) officially targets .NET 6/7/8. It may still work
// on .NET 10 via .NET Standard compatibility, but if you hit build or runtime errors,
// upgrade to Steeltoe 4.x (targets .NET 8+). The 4.x AddDiscoveryClient() call is the same.
// Steeltoe 4.x migration guide: https://docs.steeltoe.io
//builder.Services.AddDiscoveryClient(builder.Configuration);



var app = builder.Build();

// Create tables if they don't exist (requires the database itself to already exist)
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<OrderServiceDbContext>();
    db.Database.EnsureCreated();
}

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();           // serves spec at /openapi/v1.json
    app.MapScalarApiReference(); // serves UI at /scalar/v1
}

app.UseCors(options => options.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Optional test route
app.MapGet("/security/getMessage", () => "Hello World!").RequireAuthorization();

// Required if Eureka is enabled (Steeltoe 3.x only — obsolete/removed in Steeltoe 4.x)
//app.UseDiscoveryClient();

app.Run();
