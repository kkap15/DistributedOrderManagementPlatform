using System;
using System.Threading.Tasks;
using UserService.Helpers;
using UserService.Models;
using Xunit;

namespace UserService.Tests;

public class UserHelperTests
{
    private static UserClaims DefaultClaims() => new()
    {
        Auth0IdClaim = "auth0|abc",
        EmailClaim   = "test@example.com",
        NameClaim    = "Test User"
    };

    [Fact]
    public async Task CreateNewUser_MapsAllClaims()
    {
        var claims = DefaultClaims();
        var user = await UserHelper.CreateNewUser(claims);

        Assert.Equal(claims.Auth0IdClaim, user.Auth0Id);
        Assert.Equal(claims.EmailClaim,   user.Email);
        Assert.Equal(claims.NameClaim,    user.Name);
    }

    [Fact]
    public async Task CreateNewUser_AssignsNonEmptyGuid()
    {
        var user = await UserHelper.CreateNewUser(DefaultClaims());
        Assert.NotEqual(Guid.Empty, user.Id);
    }

    [Fact]
    public async Task CreateNewUser_SetsCreatedAtToUtcNow()
    {
        var before = DateTime.UtcNow;
        var user = await UserHelper.CreateNewUser(DefaultClaims());
        var after = DateTime.UtcNow;

        Assert.InRange(user.CreatedAt, before, after);
    }

    [Fact]
    public async Task CreateNewUser_NullName_UsesEmail()
    {
        var claims = DefaultClaims();
        claims.NameClaim = null!;

        var user = await UserHelper.CreateNewUser(claims);

        Assert.Equal(claims.EmailClaim, user.Name);
    }
}
