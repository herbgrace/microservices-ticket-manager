using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Steeltoe.Discovery.Eureka;

using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// OpenAPI docs (built-in .NET 10)
// UI available at: /scalar/v1
builder.Services.AddOpenApi();

// Controllers + JSON serialization
builder.Services.AddControllers().AddNewtonsoftJson(options =>
    options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore);

// IHttpClientFactory — used by AuthController to call UserService for credential verification
// UserServiceUrl is configured in appsettings.json / appsettings.Development.json
builder.Services.AddHttpClient();

// JWT Auth — AuthService is the issuer of JWT tokens
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
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero,
        ValidateIssuerSigningKey = true
    };
});
builder.Services.AddAuthorization();

// Eureka / Steeltoe Service Discovery (currently disabled)
// To re-enable: uncomment the line below.
// NOTE: Steeltoe 3.2.8 officially targets .NET 6/7/8. May still work on .NET 10 via .NET Standard
// compatibility, but if you hit errors upgrade to Steeltoe 4.x: https://docs.steeltoe.io
// builder.Services.AddDiscoveryClient(builder.Configuration);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();            // serves spec at /openapi/v1.json
    app.MapScalarApiReference(); // serves UI at /scalar/v1
}

app.UseCors(options => options.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Required if Eureka is enabled (Steeltoe 3.x only — obsolete/removed in Steeltoe 4.x)
//app.UseDiscoveryClient();

app.Run();