using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Moq;
using PaymentService.Controllers;
using PaymentService.Models;
using PaymentService.Repositories;
using Xunit;

namespace PaymentService.Tests;

public class PaymentControllerTests
{
    private readonly Mock<IPaymentRepositories> _repo = new();

    private PaymentController Build() => new(_repo.Object);

    [Fact]
    public async Task ProcessPayment_ReturnsOkWithNewGuids()
    {
        var result = await Build().ProcessPayment();

        var ok = Assert.IsType<OkObjectResult>(result);
        var payment = Assert.IsType<Payment>(ok.Value);
        Assert.NotEqual(Guid.Empty, payment.Id);
        Assert.NotEqual(Guid.Empty, payment.TransactionId);
        Assert.Equal("Success", payment.Status);
    }

    [Fact]
    public async Task ProcessPayment_PersistsAndSaves()
    {
        await Build().ProcessPayment();

        _repo.Verify(r => r.AddPaymentAsync(It.IsAny<Payment>()), Times.Once);
        _repo.Verify(r => r.SaveAsync(), Times.Once);
    }

    [Fact]
    public async Task GetAllPayments_ReturnsList()
    {
        var payments = new List<Payment> { new() { Id = Guid.NewGuid() } };
        _repo.Setup(r => r.GetAllPaymentsAsync()).ReturnsAsync(payments);

        var result = await Build().GetAllPayments();

        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(payments, ok.Value);
    }

    [Fact]
    public async Task GetPaymentById_Found_ReturnsOk()
    {
        var id = Guid.NewGuid();
        var payment = new Payment { Id = id };
        _repo.Setup(r => r.GetPaymentByIdAsync(id)).ReturnsAsync(payment);

        var result = await Build().GetPaymentById(id);

        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(payment, ok.Value);
    }

    [Fact]
    public async Task GetPaymentById_NotFound_ReturnsNotFound()
    {
        _repo.Setup(r => r.GetPaymentByIdAsync(It.IsAny<Guid>())).ReturnsAsync((Payment?)null);

        var result = await Build().GetPaymentById(Guid.NewGuid());

        Assert.IsType<NotFoundResult>(result);
    }
}
