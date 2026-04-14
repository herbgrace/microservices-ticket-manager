using System.ComponentModel.DataAnnotations;

public class Order
{
    [Key]
    public Guid OrderGuid { get; set; }

    [Required]
    public Guid UserGuid { get; set; } // Soft reference to UserService — no FK constraint

    [Required]
    public Guid BasketGuid { get; set; }
    
    [Required]
    public DateTime CreatedDate { get; set; }

    [Required]
    public List<Ticket> Tickets { get; set; } = new();

}