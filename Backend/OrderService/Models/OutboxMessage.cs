using System;
using System.ComponentModel.DataAnnotations;

namespace OrderService.Models;

public class OutboxMessage
{
    public Guid Id { get; set; }
    [Required]
    public string Topic { get; set; }
    [Required]
    public string Payload { get; set; }
    public bool IsPublished { get; set; }
    public DateTime CreatedAt { get; set; }
}