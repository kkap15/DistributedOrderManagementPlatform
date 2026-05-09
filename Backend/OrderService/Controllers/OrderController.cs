using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using OrderService.Models;
using OrderService.Repositories;
using OrderService.Services;

namespace OrderService.Controllers
{
    [ApiController]
    [Route("api/order")]
    public class OrderController(IPaymentClient paymentClient, IOrderRepositories orderRepositories, ILogger<OrderController> _logger) : ControllerBase
    {
        [HttpPost("create")]
        public async Task<IActionResult> CreateOrder(Order order)
        {
            if (order == null)
            {
                return BadRequest(new { message = "Invalid order payload"});
            }

            _logger.LogInformation($"Order received: {order?.OrderNumber}");
            try
            {
                var result = await paymentClient.ProcessPayment();
                order.Id = Guid.NewGuid();
                order.OrderNumber = $"ORD-{DateTime.UtcNow.Ticks}";
                order.TransactionId = result.TransactionId;
                order.CreatedAt = result.ProcessedAt;
                order.Status = result.Status;
                await orderRepositories.AddOrderAsync(order);
                await orderRepositories.SaveAsync();
                return Ok(result);
            }
            catch (Exception e)
            {
                throw new Exception($"Failed to create order: {e.Message}");
            }
        }

        [HttpGet("get")]
        public async Task<IActionResult> Get([FromQuery] Guid? userId)
        {
            var orders = userId.HasValue
                ? await orderRepositories.GetOrdersByUserIdAsync(userId.Value)
                : await orderRepositories.GetAllOrdersAsync();
            if (!orders.Any())
            {
                return BadRequest(new
                {
                    message = $"No Orders Created By User with {userId}"
                });
            }
            return Ok(orders);
        }
        
        [HttpGet("transactionId")]
        public async Task<IActionResult> GetOrderByTransactionId(Guid transactionId)
        {
            var order = await paymentClient.GetPaymentResponse(transactionId);
            if (order == null)
            {
                return NotFound(new { message = "No order found for the specified transaction ID." });
            }
            return Ok(order);
        }
    }
}
