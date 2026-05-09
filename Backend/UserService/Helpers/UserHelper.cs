using System;
using System.Threading.Tasks;
using UserService.Models;

namespace UserService.Helpers;

public static class UserHelper
{
    public static async Task<User> CreateNewUser(UserClaims customUserClaims)
    {
        var newUser = new User
        {
            Id = Guid.NewGuid(),
            Name = string.IsNullOrEmpty(customUserClaims.NameClaim) ? customUserClaims.EmailClaim : customUserClaims.NameClaim,
            Email = customUserClaims.EmailClaim,
            Auth0Id = customUserClaims.Auth0IdClaim,
            CreatedAt = DateTime.UtcNow
        };

        return newUser;
    }
}
