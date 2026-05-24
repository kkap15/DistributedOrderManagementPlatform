using System;

namespace PaymentService.Models
{
    public class Payment
    {
        public Guid Id { get; set; }
        public Guid TransactionId { get; set; }
        public string Status { get; set; }
        public DateTime ProcessedAt { get; set; }
        public string OrderId { get; set; }
    }
}