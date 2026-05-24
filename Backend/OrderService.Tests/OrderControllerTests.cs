using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Contracts.Messaging;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using OrderService.Controllers;
using OrderService.Models;
using OrderService.Repositories;
using Xunit;

namespace OrderService.Tests;

public class OrderControllerTests
{
    private readonly Mock<IOrderRepositories> _repo = new();
    private readonly Mock<ILogger<OrderController>> _logger = new();
    private readonly Mock<IEventPublisher> _eventPublisher = new();

    private OrderController Build() => new(_repo.Object, _logger.Object,  _eventPublisher.Object);

    [Fact]
    public async Task CreateOrder_NullOrder_ReturnsBadRequest()
    {
        var result = await Build().CreateOrder(null!);
        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task CreateOrder_ValidOrder_CallsPaymentAndSaves()
    {
        var order = new Order { UserId = Guid.NewGuid(), TotalAmount = 100 };
        var result = await Build().CreateOrder(order);
        
        _repo.Verify(r => r.AddOrderAsync(order), Times.Once);
        _repo.Verify(r => r.SaveAsync(), Times.Once);
        _eventPublisher.Verify(p => p.PublishEventAsync(
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<It.IsAnyType>(),
            It.IsAny<CancellationToken>()), Times.Once);
        
        Assert.IsType<AcceptedResult>(result);
    }

    [Fact]
    public async Task CreateOrder_PaymentThrows_ThrowsException()
    {
        _eventPublisher.Setup(p => p.PublishEventAsync(
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<It.IsAnyType>(),
            It.IsAny<CancellationToken>())).ThrowsAsync(new Exception("kafka down"));
        
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
