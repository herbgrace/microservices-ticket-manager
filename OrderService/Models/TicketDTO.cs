
using System.ComponentModel.DataAnnotations;

public class TicketDTO
{    
    [Required]
    public Guid? TicketGuid { get; set; }

    [Required]
    public string? Event { get; set; }

    [Required]
    public string? Description { get; set; }

    [Range(0.01, double.MaxValue)]
    public decimal Price { get; set; }      

    [Required]
    public DateTime EventDate { get; set; }

}