using PaymentService.Data;
using PaymentService.Models;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using System.Collections.Generic;
using System;

namespace PaymentService.Repositories;

public class PaymentRepositories : IPaymentRepositories
{
    private readonly PaymentDbContext _paymentDbContext;

    public PaymentRepositories(PaymentDbContext paymentDbContext)
    {
        _paymentDbContext = paymentDbContext;
    }

    public async Task AddPaymentAsync(Payment payment)
    {
        await _paymentDbContext.Payments.AddAsync(payment);
    }
    

    public async Task<Payment?> GetPaymentByIdAsync(Guid id)
    {
        return await _paymentDbContext.Payments.FindAsync(id);
    }
    
    public async Task<IEnumerable<Payment>> GetAllPaymentsAsync()
    {
        return await _paymentDbContext.Payments.ToListAsync();
    }
    
    public async Task SaveAsync()
    {
        await _paymentDbContext.SaveChangesAsync();
    }
}