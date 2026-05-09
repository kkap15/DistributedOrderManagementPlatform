using System;
using System.Text.Json.Serialization;

namespace OrderService.Models;

public class PaymentResponse
{
    [JsonPropertyName("transactionId")]
    public Guid TransactionId { get; set; }
    [JsonPropertyName("id")]
    public Guid Id { get; set; }
    [JsonPropertyName("status")]
    public string Status { get; set; }
    [JsonPropertyName("processedAt")]
    public DateTime ProcessedAt { get; set; }
}