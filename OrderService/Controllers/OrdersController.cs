using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;

[ApiController]
[Route("api/[controller]")]
public class OrdersController(
    ILogger<OrdersController> logger,
    OrderServiceDbContext db,
    IMapper mapper,
    IConfiguration config) : ControllerBase
{
    // NOTE: HttpClient should be injected via IHttpClientFactory rather than instantiated
    // directly. Direct instantiation bypasses connection pooling and can cause socket
    // exhaustion under load. Kept here as a teaching reference — see Phase 2 cleanup.
    private readonly HttpClient _httpClient = new();

    [HttpGet("test")]
    public IActionResult Test()
    {
        return Ok("Hello from OrderController - running .NET 10!");
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        try
        {
            var orders = await db.Orders.Include(o => o.Tickets).ToListAsync();
            return Ok(new { Success = true, Message = "Orders retrieved.", Orders = orders });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving orders.");
            return StatusCode(500, "Internal server error.");
        }
    }

    [HttpGet("with-tickets")]
    public async Task<IActionResult> GetAllWithTickets()
    {
        try
        {
            var orders = await db.Orders.Include(o => o.Tickets).ToListAsync();
            return Ok(new { Success = true, Message = "Orders with tickets retrieved.", Orders = orders });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving orders with tickets.");
            return StatusCode(500, "Internal server error.");
        }
    }

    [HttpGet("{orderGuid:guid}")]
    public async Task<IActionResult> GetByOrderGuid(Guid orderGuid)
    {
        try
        {
            var order = await db.Orders.Include(o => o.Tickets).FirstOrDefaultAsync(o => o.OrderGuid == orderGuid);
            if (order == null)
                return NotFound($"Order with GUID {orderGuid} not found.");

            return Ok(new { Success = true, Message = "Order retrieved.", Order = order });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving order {OrderGuid}.", orderGuid);
            return StatusCode(500, "Internal server error.");
        }
    }

    [HttpGet("user/{userGuid:guid}")]
    public async Task<IActionResult> GetByUserGuid(Guid userGuid)
    {
        try
        {
            var orders = await db.Orders.Include(o => o.Tickets).Where(o => o.UserGuid == userGuid).ToListAsync();
            return Ok(new { Success = true, Message = "Orders by user retrieved.", Orders = orders });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving orders for user {UserGuid}.", userGuid);
            return StatusCode(500, "Internal server error.");
        }
    }

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] OrderDTO orderDTO)
    {
        try
        {
            var userGuidClaim = User.Claims.FirstOrDefault(c => c.Type == "UserGuid")?.Value;
            if (string.IsNullOrEmpty(userGuidClaim))
            {
                return Unauthorized();
            }
            Guid userguid = Guid.Parse(userGuidClaim);

            var username = User.Identity?.Name; // from ClaimTypes.Name
            var useremail = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;

            var order = mapper.Map<Order>(orderDTO);
            order.UserGuid = userguid;
            order.OrderGuid = Guid.NewGuid();
            order.CreatedDate = DateTime.UtcNow;


            db.Orders.Add(order);
            await db.SaveChangesAsync();

            var notification = new OrderNotification
            {
                UserGuid = order.UserGuid,
                OrderGuid = order.OrderGuid,
                Name = username,
                Email = useremail,
                Message = $"Your AMAZING order {order.OrderGuid} was created successfully."
            };

            var messageServiceUrl = config["MessageServiceUrl"];
            if (!string.IsNullOrEmpty(messageServiceUrl))
            {
                try
                {
                    var response = await _httpClient.PostAsJsonAsync(messageServiceUrl, notification);
                    response.EnsureSuccessStatusCode();
                }
                catch (Exception ex)
                {
                    logger.LogWarning(ex, "Failed to send notification to MessageService.");
                }
            }
            else
            {
                logger.LogWarning("MessageService URL not configured — skipping notification.");
            }

            return CreatedAtAction(nameof(GetByOrderGuid), new { orderGuid = order.OrderGuid }, new
            {
                Success = true,
                Message = "Order created.",
                OrderGuid = order.OrderGuid,
                UserGuid = order.UserGuid
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error creating order.");
            return StatusCode(500, "Internal server error.");
        }
    }

    [Authorize]
    [HttpDelete("{orderGuid:guid}")]
    public async Task<IActionResult> Delete(Guid orderGuid)
    {
        try
        {
            var order = await db.Orders.Include(o => o.Tickets).FirstOrDefaultAsync(o => o.OrderGuid == orderGuid);
            if (order == null)
                return NotFound($"Order with GUID {orderGuid} not found.");

            db.Orders.Remove(order);
            await db.SaveChangesAsync();

            return Ok(new { Success = true, Message = "Order and associated tickets deleted." });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error deleting order {OrderGuid}.", orderGuid);
            return StatusCode(500, "Internal server error.");
        }
    }
}
