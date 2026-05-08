using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

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
