using System.IO;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace ApiGateway.Controllers;

[ApiController]
[Route("gateway")]
public class GatewayController(IHttpClientFactory clientFactory, ILogger<GatewayController> _logger, IConfiguration configuration) : ControllerBase
{
    private readonly HttpClient _httpClient = clientFactory.CreateClient();
    private string OrderService => configuration["Services:OrderService"]!;
    private string UserService => configuration["Services:UserService"]!;

    /*
    * Gateway intercepts the token and body and send it to 
    * OrderService to create an order.
    */
    [Authorize]
    [HttpPost]
    [Route("order")]
    public async Task<IActionResult> CreateOrder()
    {
        var token = Request.Headers["Authorization"].ToString();
        _logger.LogInformation(token);
        Request.EnableBuffering();
        var body = await new StreamReader(Request.Body).ReadToEndAsync();
        Request.Body.Position = 0;
        _logger.LogInformation($"Received order creation request with body: {body}");
        var request = new HttpRequestMessage(new HttpMethod("POST"), $"{OrderService}/api/order/create");
        request.Content = new StringContent(body, Encoding.UTF8, "application/json");
        request.Headers.Add("Authorization", token);
        var response = await _httpClient.SendAsync(request);
        var result = await response.Content.ReadAsStringAsync();
        if (string.IsNullOrEmpty(result))
        {
            return BadRequest(new { message = "Order Creation Failed." });
        }

        object jsonObject;
        try
        {
            jsonObject = JsonSerializer.Deserialize<object>(result);
        }
        catch
        {
            return BadRequest(new
            {
                message = "Invalid json response",
                raw = result
            });
        }
        
        return new ObjectResult(jsonObject)
        {
            StatusCode = (int)response.StatusCode
        };
    }
    
    [Authorize]
    [HttpGet("order")]
    public async Task<IActionResult> GetOrders()
    {
        var qs = Request.QueryString.Value ?? "";
        var request = new HttpRequestMessage(new HttpMethod("GET"), $"{OrderService}/api/order/get{qs}");
        var response = await _httpClient.SendAsync(request);
        var result = await response.Content.ReadAsStringAsync();
        _logger.LogInformation("Received GET request at API Gateway");
        return Ok(JsonSerializer.Deserialize<object>(result));
    }
    
    [Authorize]
    [HttpPost("user/login")]
    public async Task<IActionResult> UserLogin()
    {
        var token = Request.Headers.Authorization.ToString();
        var request = new HttpRequestMessage(HttpMethod.Post, $"{UserService}/api/user/login");
        request.Headers.Add("Authorization", token);
        var response = await _httpClient.SendAsync(request);
        var result = await response.Content.ReadAsStringAsync();
        return new ObjectResult(JsonSerializer.Deserialize<object>(result))
        {
            StatusCode = (int)response.StatusCode
        };
    }
    
    [Authorize]
    [HttpGet("user/me")]
    public async Task<IActionResult> GetMe()
    {
        var token = Request.Headers.Authorization.ToString();
        var request = new HttpRequestMessage(HttpMethod.Get, $"{UserService}/api/user/me");
        request.Headers.Add("Authorization", token);
        var response = await _httpClient.SendAsync(request);
        var result = await response.Content.ReadAsStringAsync();
        return new ObjectResult(JsonSerializer.Deserialize<object>(result))
        {
            StatusCode = (int)response.StatusCode
        };
    }
}