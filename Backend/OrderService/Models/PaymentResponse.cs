using System;
using System.Text.Json.Serialization;

namespace OrderService.Models;

public class PaymentResponse
{
    [JsonPropertyName("message")]
    public string Message { get; set; }
    [JsonPropertyName("transactionId")]
    public Guid TransactionId { get; set; }
}