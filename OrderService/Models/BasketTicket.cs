using System;
using System.Text.Json.Serialization;

public class BasketTicket
{
    [JsonPropertyName("id")]
    public Guid Id { get; set; }

    [JsonPropertyName("event")]
    public string Event { get; set; } = string.Empty;

    [JsonPropertyName("price")]
    public decimal Price { get; set; }

    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

    [JsonPropertyName("eventDate")]
    public DateTime EventDate { get; set; }
}