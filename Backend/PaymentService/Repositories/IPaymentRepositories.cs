using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using PaymentService.Models;

namespace PaymentService.Repositories;

public interface IPaymentRepositories
{
    Task AddPaymentAsync(Payment payment);
    Task<Payment> GetPaymentByIdAsync(Guid id);
    Task<IEnumerable<Payment>> GetAllPaymentsAsync();
    Task SaveAsync();
}