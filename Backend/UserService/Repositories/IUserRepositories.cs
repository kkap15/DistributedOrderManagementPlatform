using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UserService.Models;

namespace UserService.Repositories;

public interface IUserRepositories
{
    Task AddUserAsync(Models.User user);
    Task<User> GetUserByIdAsync(Guid id);
    Task<IEnumerable<Models.User>> GetAllUsersAsync();
    Task SaveAsync();
    Task<User?> GetByAuth0IdAsync(string auth0Id);
}