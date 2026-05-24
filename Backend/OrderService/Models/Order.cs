using System;
using System.ComponentModel.DataAnnotations;

namespace OrderService.Models
{
    public class Order
    {
        [Key]
        public Guid Id { get; set; }
        public string OrderNumber { get; set; }
        public string Status { get; set; }
        public Guid? TransactionId { get; set; }
        public Guid UserId { get; set; }
        public DateTime CreatedAt { get; set; }
        public decimal TotalAmount { get; set; }
    }
}
