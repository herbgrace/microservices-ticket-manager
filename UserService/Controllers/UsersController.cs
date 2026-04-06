using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;

[ApiController]
[Route("api/[controller]")]
public class UsersController(
    ILogger<UsersController> logger,
    UserServiceDbContext db,
    IMapper mapper) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        try
        {
            var users = await db.Users.ToListAsync();
            return Ok(new { Success = true, Message = "All users returned.", Users = users });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving users.");
            return StatusCode(500, "Internal server error.");
        }
    }

    [HttpGet("{userGuid:guid}")]
    public async Task<IActionResult> GetById(Guid userGuid)
    {
        try
        {
            var user = await db.Users.FirstOrDefaultAsync(u => u.UserGuid == userGuid);
            if (user == null)
                return NotFound(new { Success = false, Message = "User not found." });

            return Ok(new { Success = true, Message = "User fetched.", User = user });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error fetching user {UserGuid}.", userGuid);
            return StatusCode(500, "Internal server error.");
        }
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] UserDTO userDTO)
    {
        try
        {
            var existingUser = await db.Users.FirstOrDefaultAsync(u => u.Email == userDTO.Email);
            if (existingUser is not null)
                return Ok(new
                {
                    Success = true,
                    Message = "User already exists.",
                    UserGuid = existingUser.UserGuid,
                    Email = existingUser.Email
                });

            var user = mapper.Map<User>(userDTO);
            user.Username ??= userDTO.Email;
            user.CreatedDate = DateTime.UtcNow;
            user.PasswordHash = HashPassword(userDTO.Password);

            await db.Users.AddAsync(user);
            await db.SaveChangesAsync();

            logger.LogInformation("User created with GUID {UserGuid}.", user.UserGuid);
            return Ok(new { Success = true, Message = "User created.", UserGuid = user.UserGuid });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error creating user.");
            return StatusCode(500, "Internal server error.");
        }
    }

    // Internal endpoint called by AuthService to verify credentials and return safe user info.
    // Returns UserGuid, Username, and Email — never exposes PasswordHash over the wire.
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] UserDTO userDTO)
    {
        try
        {
            var user = await db.Users.FirstOrDefaultAsync(u => u.Email == userDTO.Email);
            if (user == null)
            {
                logger.LogWarning("Login attempt failed — email not found: {Email}.", userDTO.Email);
                return Unauthorized();
            }

            if (!VerifyPassword(user.PasswordHash, userDTO.Password))
            {
                logger.LogWarning("Login attempt failed — invalid password for {Email}.", userDTO.Email);
                return Unauthorized();
            }

            logger.LogInformation("Login successful for {Email}.", userDTO.Email);
            return Ok(new { user.UserGuid, user.Username, user.Email });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error during login for {Email}.", userDTO.Email);
            return StatusCode(500, "Internal server error.");
        }
    }

    [HttpPut("{userGuid:guid}")]
    public async Task<IActionResult> Update(Guid userGuid, [FromBody] UserDTO userDTO)
    {
        try
        {
            var user = await db.Users.FirstOrDefaultAsync(u => u.UserGuid == userGuid);
            if (user is null)
                return NotFound(new { Success = false, Message = "User not found." });

            var existingUsername = user.Username;
            mapper.Map(userDTO, user);
            user.Username ??= existingUsername;
            await db.SaveChangesAsync();

            logger.LogInformation("User {UserGuid} updated.", userGuid);
            return Ok(new { Success = true, Message = "User updated.", UserGuid = user.UserGuid });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error updating user {UserGuid}.", userGuid);
            return StatusCode(500, "Internal server error.");
        }
    }

    [HttpDelete("{userGuid:guid}")]
    public async Task<IActionResult> Delete(Guid userGuid)
    {
        try
        {
            var user = await db.Users.FirstOrDefaultAsync(u => u.UserGuid == userGuid);
            if (user is null)
                return NotFound(new { Success = false, Message = "User not found." });

            db.Users.Remove(user);
            await db.SaveChangesAsync();

            logger.LogInformation("User {UserGuid} deleted.", userGuid);
            return Ok(new { Success = true, Message = "User deleted.", UserGuid = user.UserGuid });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error deleting user {UserGuid}.", userGuid);
            return StatusCode(500, "Internal server error.");
        }
    }

    private static string HashPassword(string password)
    {
        var passwordHasher = new PasswordHasher<object>();
        return passwordHasher.HashPassword(null!, password);
    }

    private static bool VerifyPassword(string hashedPassword, string enteredPassword)
    {
        var passwordHasher = new PasswordHasher<object>();
        var result = passwordHasher.VerifyHashedPassword(null!, hashedPassword, enteredPassword);
        return result == PasswordVerificationResult.Success;
    }
}
