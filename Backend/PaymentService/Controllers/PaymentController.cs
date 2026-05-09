using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using PaymentService.Models;
using PaymentService.Repositories;

namespace PaymentService.Controllers
{
    [ApiController]
    [Route("api/payment")]
    public class PaymentController(IPaymentRepositories paymentRepository) : ControllerBase
    {
        [HttpPost]
        public async Task<IActionResult> ProcessPayment()
        {
            var response = new Payment
            {
                Id = Guid.NewGuid(),
                TransactionId = Guid.NewGuid(),
                Status = "Success",
                ProcessedAt = DateTime.UtcNow
            };
            await paymentRepository.AddPaymentAsync(response);
            await paymentRepository.SaveAsync();
            return Ok(response);
        }
        
        [HttpGet]
        public async Task<IActionResult> GetAllPayments()
        {
            var payments = await paymentRepository.GetAllPaymentsAsync();
            return Ok(payments);
        }
        
        [HttpGet("{id}")]
        public async Task<IActionResult> GetPaymentById(Guid id)
        {
            var payment = await paymentRepository.GetPaymentByIdAsync(id);
            if (payment == null)
            {
                return NotFound();
            }
            return Ok(payment);
        }
    }
}
