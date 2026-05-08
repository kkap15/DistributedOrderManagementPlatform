using System;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ApiGateway.Controllers;

[ApiController]
[Route("gateway/order")]
public class GatewayController(IHttpClientFactory clientFactory) : ControllerBase
{
    private readonly HttpClient _httpClient = clientFactory.CreateClient();

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> Forward()
    {
        var token = Request.Headers["Authorization"].ToString();
        
        Request.EnableBuffering();
        var body = await new StreamReader(Request.Body).ReadToEndAsync();
        Request.Body.Position = 0;
        var request = new HttpRequestMessage(new HttpMethod("POST"), $"http://localhost:5000/api/order");
        request.Content = new StringContent(body, Encoding.UTF8, "application/json");
        request.Headers.Add("Authorization", token);
        var response = await _httpClient.SendAsync(request);
        var result = await response.Content.ReadAsStringAsync();
        if (string.IsNullOrEmpty(result))
        {
            return Ok(new { message = "Order created successfully" });
        }

        object jsonObject;
        try
        {
            jsonObject = JsonSerializer.Deserialize<object>(result);
        }
        catch
        {
            return Ok(new
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
    [HttpGet]
    public IActionResult Get()
    {
        Console.WriteLine("Received GET request at API Gateway");
        return Ok("Hello World!");
    }
}