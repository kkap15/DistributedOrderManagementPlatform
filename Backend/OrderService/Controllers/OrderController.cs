using System;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Contracts;
using Contracts.Events;
using Contracts.Messaging;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using OrderService.Models;
using OrderService.Repositories;

namespace OrderService.Controllers
{
    [ApiController]
    [Route("api/order")]
    public class OrderController(IOrderRepositories orderRepositories, ILogger<OrderController> _logger) : ControllerBase
    {
        [HttpPost("create")]
        public async Task<IActionResult> CreateOrder(Order order)
        {
            if (order == null)
            {
                return BadRequest(new { message = "Invalid order payload"});
            }

            order.TransactionId = null;
            order.Id = Guid.NewGuid();
            order.OrderNumber = $"ORD-{DateTime.Now.Ticks}";
            order.CreatedAt = DateTime.UtcNow;
            order.Status = "Pending";
            
            try
            {
                var payload = JsonSerializer.Serialize(new OrderCreatedEvent(order.Id.ToString(),
                    order.UserId.ToString(), order.TotalAmount, order.CreatedAt));
                var outboxMessage = new OutboxMessage
                {
                    Id = Guid.NewGuid(),
                    Topic = Topics.OrderCreated,
                    Payload = payload,
                    IsPublished = false,
                    CreatedAt = DateTime.UtcNow
                };
                await orderRepositories.AddOutboxMessageAsync(outboxMessage);
                await orderRepositories.AddOrderAsync(order);
                await orderRepositories.SaveAsync();
                
                _logger.LogInformation("Order {OrderNumber} created with status Pending", order.OrderNumber);
                return Accepted(new { orderId = order.Id, status = "Pending"});
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to create order {OrderNumber}", order.OrderNumber);
                throw;
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
            var order = await orderRepositories.GetOrderByIdAsync(transactionId);
            if (order == null)
            {
                return NotFound(new { message = "No order found for the specified transaction ID." });
            }
            return Ok(order);
        }
    }
}
