
using System.ComponentModel.DataAnnotations;

public class TicketDTO
{    
    public Guid TicketGuid { get; set; } 
    public required string Event { get; set; }
    public required string Description { get; set; }
    public decimal Price { get; set; }      
    public DateTime EventDate { get; set; }

}