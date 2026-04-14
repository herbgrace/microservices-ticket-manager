
using System.ComponentModel.DataAnnotations;

public class Ticket
{
    [Key]
    public Guid TicketGuid { get; set; }

    [Required]
    public Guid OrderGuid { get; set; }

    [Required]
    public Order Order { get; set; } = null!;

    [Required]
    public required string Event { get; set; }

    [Required]
    public required string Description { get; set; }

    [Required]
    public decimal Price { get; set; }

    [Required]
    public DateTime EventDate { get; set; }
}