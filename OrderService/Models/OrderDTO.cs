
using System;
using System.ComponentModel.DataAnnotations;

public class OrderDTO
{
    [Required]
    public Guid? UserGuid { get; set; }

    public Guid OrderGuid { get; set; }

    public bool ReadBasket { get; set; }

    [Required]
    [MinLength(1)]
    public List<TicketDTO>? Tickets { get; set; }
}