using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using OrderService.Data;
using OrderService.Models;

namespace OrderService.Repositories;

public class OrderRepositories : IOrderRepositories
{
    private readonly OrderDbContext _context;

    public OrderRepositories(OrderDbContext context)
    {
        _context = context;
    }

    public async Task AddOrderAsync(Order order)
    {
        await _context.Orders.AddAsync(order);
    }

    public async Task<Order?> GetOrderByIdAsync(Guid id)
    {
        return await _context.Orders.FirstOrDefaultAsync(o => o.Id == id);
    }

    public async Task<IEnumerable<Order>> GetAllOrdersAsync()
    {
        return await _context.Orders.ToListAsync();
    }

    public async Task SaveAsync()
    {
        await _context.SaveChangesAsync();
    }
    
    public async Task<IEnumerable<Order>> GetOrdersByUserIdAsync(Guid userId)
    {
        return await _context.Orders.Where(o => o.UserId == userId).ToListAsync();
    }

    public async Task AddOutboxMessageAsync(OutboxMessage message)
    {
        await _context.OutboxMessages.AddAsync(message);
    }

    public async Task<List<OutboxMessage>> GetUnpublishedOutboxMessagesAsync()
    {
        return await _context.OutboxMessages
            .Where(m => !m.IsPublished)
            .OrderBy(m => m.CreatedAt)
            .ToListAsync();
    }
}