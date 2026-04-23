using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

// Response shape returned by UserService POST /api/users/login
internal record LoginResponse(Guid UserGuid, string Username, string Email);

[ApiController]
[Route("api")]
public class AuthController(
    ILogger<AuthController> logger,
    IHttpClientFactory httpClientFactory,
    IConfiguration config) : ControllerBase
{
    [HttpGet("test1")]
    public IActionResult Test1() => Ok("Hello from AuthController");

    // Accepts email + password, calls UserService to verify credentials,
    // and returns a signed JWT on success.
    [HttpPost("createtoken")]
    public async Task<IActionResult> CreateTokenMethod1([FromBody] UserDTO userDto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var userServiceUrl = config["UserServiceUrl"]
            ?? throw new InvalidOperationException("UserServiceUrl is not configured.");

        var httpClient = httpClientFactory.CreateClient();

        // Delegate credential verification to UserService — it owns the Users table
        // and the password hashing logic. AuthService never touches the database.
        HttpResponseMessage response;
        try
        {
            response = await httpClient.PostAsJsonAsync($"{userServiceUrl}/api/login", userDto);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to reach UserService at {UserServiceUrl}.", userServiceUrl);
            return StatusCode(503, "UserService is unavailable.");
        }

        if (!response.IsSuccessStatusCode)
        {
            logger.LogWarning("Login failed for {Email} — UserService returned {StatusCode}.",
                userDto.Email, (int)response.StatusCode);
            return Unauthorized();
        }

        var userInfo = await response.Content.ReadFromJsonAsync<LoginResponse>();
        if (userInfo == null)
        {
            logger.LogError("UserService returned success but response body was empty for {Email}.", userDto.Email);
            return StatusCode(500, "Unexpected response from UserService.");
        }

        var claims = new List<Claim>
        {
            new(ClaimTypes.Name, userInfo.Username),
            new(ClaimTypes.Email, userInfo.Email),
            new("UserGuid", userInfo.UserGuid.ToString())
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["Jwt:Key"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: config["Jwt:Issuer"],
            audience: config["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddHours(3),
            signingCredentials: creds);

        var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

        logger.LogInformation("JWT issued for {Email}.", userDto.Email);
        return Ok(tokenString);
    }
}