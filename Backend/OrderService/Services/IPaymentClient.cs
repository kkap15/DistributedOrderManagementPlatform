using System;
using System.Threading.Tasks;
using OrderService.Models;

namespace OrderService.Services;

public interface IPaymentClient
{
    Task<PaymentResponse> ProcessPayment();
    Task<PaymentResponse> GetPaymentResponse(Guid transactionId);
}
