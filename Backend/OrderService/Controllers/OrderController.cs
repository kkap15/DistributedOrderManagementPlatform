using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using OrderService.Models;
using OrderService.Services;

namespace OrderService.Controllers
{
    [ApiController]
    [Route("api/order")]
    public class OrderController(PaymentClient paymentClient) : ControllerBase
    {
        [HttpPost]
        public async Task<IActionResult> CreateOrder(Order order)
        {
            if (order == null)
            {
                return BadRequest("Invalid order payload");
            }

            Console.WriteLine($"Order received: {order?.OrderNumber}");
            try
            {
                var result = await paymentClient.ProcessPayment();
                Console.WriteLine($"Payment result: {result?.Message}");
                Console.WriteLine(result!.TransactionId.ToString());
                return Ok((object)result ?? new
                {
                    message = "Payment returned null",
                    transactionId = Guid.Empty
                });
            }
            catch (Exception e)
            {
                throw new Exception($"Failed to create order: {e.Message}");
            }
        }

        [HttpGet]
        public IActionResult Get()
        {
            Console.WriteLine("Received GET request at Order Service");
            return Ok("Hello World!");
        }
    }
}
