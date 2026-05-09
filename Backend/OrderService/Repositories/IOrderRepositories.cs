using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using OrderService.Models;

namespace OrderService.Repositories;

public interface IOrderRepositories
{
    Task AddOrderAsync(Order order);
    Task<Order> GetOrderByIdAsync(Guid id);
    Task<IEnumerable<Order>> GetAllOrdersAsync();
    Task SaveAsync();
    Task<IEnumerable<Order>> GetOrdersByUserIdAsync(Guid userId);
}