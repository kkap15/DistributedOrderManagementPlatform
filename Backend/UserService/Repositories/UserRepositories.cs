using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UserService.Data;
using Microsoft.EntityFrameworkCore;
using UserService.Models;

namespace UserService.Repositories;

public class UserRepositories : IUserRepositories
{
    private readonly UserDbContext _context;

    public UserRepositories(UserDbContext context)
    {
        _context = context;
    }

    public async Task AddUserAsync(User user)
    {
        await _context.Users.AddAsync(user);
    }

    public async Task<User> GetUserByIdAsync(Guid id)
    {
        return await _context.Users.FindAsync(id);
    }

    public async Task<IEnumerable<User>> GetAllUsersAsync()
    {
        return await _context.Users.ToListAsync();
    }

    public async Task SaveAsync()
    {
        await _context.SaveChangesAsync();
    }
    
    public async Task<User?> GetByAuth0IdAsync(string auth0Id)
    {
        return await _context.Users.FirstOrDefaultAsync(u => u.Auth0Id == auth0Id);
    }
}