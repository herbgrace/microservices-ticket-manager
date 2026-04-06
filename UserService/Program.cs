using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

// eureka
using Steeltoe.Discovery.Client;
using Steeltoe.Discovery.Eureka;

using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// OpenAPI docs (built-in .NET 10)
// UI available at: /scalar/v1
builder.Services.AddOpenApi();

// EF Core — UserService owns the UserServiceDB database and the Users table
builder.Services.AddDbContext<UserServiceDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("conn_userservice_sqlserver")));

// Controllers + JSON serialization
builder.Services.AddControllers().AddNewtonsoftJson(options =>
    options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore);

// AutoMapper
builder.Services.AddAutoMapper(cfg => cfg.AddProfile<AutoMapperProfile>());

// JWT Auth — used for any endpoints that require authorization
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

//if you hit errors upgrade to Steeltoe 4.x: https://docs.steeltoe.io
//builder.Services.AddDiscoveryClient(builder.Configuration);

var app = builder.Build();

// UserService owns the schema — create Users table if it doesn't exist.
// Requires UserServiceDB to already exist on the SQL Server instance.
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<UserServiceDbContext>();
    db.Database.EnsureCreated();
}

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
