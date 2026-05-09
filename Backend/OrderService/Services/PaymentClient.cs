using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using OrderService.Models;
using Polly;
using Polly.CircuitBreaker;
using Polly.Retry;

namespace OrderService.Services;

public class PaymentClient(HttpClient httpClient) : IPaymentClient
{
    private static readonly JsonSerializerOptions _jsonOptions = new() { PropertyNameCaseInsensitive = true };
    
    private readonly ResiliencePipeline<HttpResponseMessage> _pipeline =
        new ResiliencePipelineBuilder<HttpResponseMessage>()
            .AddRetry(new RetryStrategyOptions<HttpResponseMessage>
            {
                MaxRetryAttempts = 3,
                BackoffType = DelayBackoffType.Exponential,
                Delay = TimeSpan.FromSeconds(2),
                ShouldHandle = new PredicateBuilder<HttpResponseMessage>()
                    .Handle<HttpRequestException>()
                    .HandleResult(r => !r.IsSuccessStatusCode),
                OnRetry = args =>
                {
                    Console.WriteLine($"Retry {args.AttemptNumber} after {args.RetryDelay.Seconds}s");
                    return default;
                }
            })
            .AddCircuitBreaker(new CircuitBreakerStrategyOptions<HttpResponseMessage>
            {
                FailureRatio = 0.5,
                MinimumThroughput = 2,
                SamplingDuration = TimeSpan.FromSeconds(30),
                BreakDuration = TimeSpan.FromSeconds(30),
                ShouldHandle = new PredicateBuilder<HttpResponseMessage>()
                    .Handle<HttpRequestException>()
                    .HandleResult(r => !r.IsSuccessStatusCode),
                OnOpened = _ => { Console.WriteLine("Circuit Open"); return default; },
                OnClosed = _ => { Console.WriteLine("Circuit Closed"); return default; }
            })
            .Build();
    public async Task<PaymentResponse> ProcessPayment()
    {
        var response = await _pipeline.ExecuteAsync(async ct =>
            await httpClient.PostAsync("/api/payment", null, ct));

        var result = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<PaymentResponse>(result, _jsonOptions);
    }
    
    public async Task<PaymentResponse> GetPaymentResponse(Guid transactionId)
    {
        var response = await _pipeline.ExecuteAsync(async ct =>
            await httpClient.GetAsync($"/api/payment/{transactionId}", ct));

        var result = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<PaymentResponse>(result, _jsonOptions);
    }
}