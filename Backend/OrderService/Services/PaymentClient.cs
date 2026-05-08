using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using OrderService.Models;
using Polly;
using Polly.CircuitBreaker;

namespace OrderService.Services;

public class PaymentClient(HttpClient httpClient)
{
    public async Task<PaymentResponse> ProcessPayment()
    {
        var retryPolicy = Policy
            .Handle<HttpRequestException>()
            .OrResult<HttpResponseMessage>(response => !response.IsSuccessStatusCode)
            .WaitAndRetryAsync(
                3,
                retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                (exception, timeSpan, retryCount, context) =>
                {
                    Console.WriteLine($"Retry {retryCount} after {timeSpan.Seconds} seconds due to: {exception.Exception?.Message ?? exception.Result?.StatusCode.ToString()}");
                });
        var circuitBreaker = Policy
            .Handle<HttpRequestException>()
            .OrResult<HttpResponseMessage>(response => !response.IsSuccessStatusCode)
            .CircuitBreakerAsync(
                2,
                TimeSpan.FromSeconds(30),
                onBreak: (ex, breakDelay) => { Console.WriteLine("Circuit Open"); },
                onReset: () => { Console.WriteLine("Circuit Closed"); });
        var policy = Policy.WrapAsync(retryPolicy, circuitBreaker);
        var response = await policy.ExecuteAsync(() =>
            httpClient.PostAsync("http://localhost:5001/api/payment", null));

        var result = await response.Content.ReadAsStringAsync();
        var json = JsonSerializer.Deserialize<PaymentResponse>(
            result,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        return json;
    }
}