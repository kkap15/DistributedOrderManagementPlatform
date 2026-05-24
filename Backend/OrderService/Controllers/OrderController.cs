using System;
using System.Linq;
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
    public class OrderController(IOrderRepositories orderRepositories, 
        ILogger<OrderController> _logger, IEventPublisher eventPublisher) : ControllerBase
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
                await orderRepositories.AddOrderAsync(order);
                await orderRepositories.SaveAsync();
                
                await eventPublisher.PublishEventAsync(
                    Topics.OrderCreated,
                    order.Id.ToString(),
                    new OrderCreatedEvent(
                        Amount: order.TotalAmount,
                        CreatedAt: order.CreatedAt,
                        OrderId:  order.Id.ToString(),
                        UserId: order.UserId.ToString()),
                    CancellationToken.None);
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
