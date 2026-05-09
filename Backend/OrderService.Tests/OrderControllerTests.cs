using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using OrderService.Controllers;
using OrderService.Models;
using OrderService.Repositories;
using OrderService.Services;
using Xunit;

namespace OrderService.Tests;

public class OrderControllerTests
{
    private readonly Mock<IPaymentClient> _payment = new();
    private readonly Mock<IOrderRepositories> _repo = new();
    private readonly Mock<ILogger<OrderController>> _logger = new();

    private OrderController Build() => new(_payment.Object, _repo.Object, _logger.Object);

    [Fact]
    public async Task CreateOrder_NullOrder_ReturnsBadRequest()
    {
        var result = await Build().CreateOrder(null!);
        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task CreateOrder_ValidOrder_CallsPaymentAndSaves()
    {
        var payment = new PaymentResponse
        {
            Id = Guid.NewGuid(),
            TransactionId = Guid.NewGuid(),
            Status = "Success",
            ProcessedAt = DateTime.UtcNow
        };
        _payment.Setup(p => p.ProcessPayment()).ReturnsAsync(payment);

        var order = new Order { UserId = Guid.NewGuid(), TotalAmount = 100 };
        var result = await Build().CreateOrder(order);

        _payment.Verify(p => p.ProcessPayment(), Times.Once);
        _repo.Verify(r => r.AddOrderAsync(order), Times.Once);
        _repo.Verify(r => r.SaveAsync(), Times.Once);
        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public async Task CreateOrder_PaymentThrows_ThrowsException()
    {
        _payment.Setup(p => p.ProcessPayment()).ThrowsAsync(new Exception("payment down"));
        var order = new Order { UserId = Guid.NewGuid() };

        await Assert.ThrowsAsync<Exception>(() => Build().CreateOrder(order));
    }

    [Fact]
    public async Task Get_NoUserId_ReturnsAllOrders()
    {
        var orders = new List<Order> { new() { Id = Guid.NewGuid() } };
        _repo.Setup(r => r.GetAllOrdersAsync()).ReturnsAsync(orders);

        var result = await Build().Get(null);

        _repo.Verify(r => r.GetAllOrdersAsync(), Times.Once);
        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(orders, ok.Value);
    }

    [Fact]
    public async Task Get_WithUserId_ReturnsFilteredOrders()
    {
        var userId = Guid.NewGuid();
        var orders = new List<Order> { new() { Id = Guid.NewGuid(), UserId = userId } };
        _repo.Setup(r => r.GetOrdersByUserIdAsync(userId)).ReturnsAsync(orders);

        var result = await Build().Get(userId);

        _repo.Verify(r => r.GetOrdersByUserIdAsync(userId), Times.Once);
        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(orders, ok.Value);
    }
}
