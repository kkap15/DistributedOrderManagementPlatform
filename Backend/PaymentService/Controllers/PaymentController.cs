using System;
using Microsoft.AspNetCore.Mvc;

namespace PaymentService.Controllers
{
    [ApiController]
    [Route("api/payment")]
    public class PaymentController : ControllerBase
    {
        [HttpPost]
        public IActionResult ProcessPayment()
        {
            var response = new
            {
                message = "Payment processed successfully",
                transactionId = Guid.NewGuid().ToString()
            };
            return Ok(response);
        }
    }
}
