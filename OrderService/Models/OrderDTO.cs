
using System;
using System.ComponentModel.DataAnnotations;

public class OrderDTO
{
    [Required]
    public string BasketGuid { get; set; }
}