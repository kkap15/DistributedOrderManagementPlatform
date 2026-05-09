using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using UserService.Controllers;
using UserService.Models;
using UserService.Repositories;
using Xunit;

namespace UserService.Tests;

public class UserControllerTests
{
    private readonly Mock<IUserRepositories> _repo = new();

    private const string EmailClaim = "http://microservices-api:email";
    private const string NameClaim  = "http://microservices-api:name";

    private UserController BuildController(string? sub = "auth0|123", string? email = "u@test.com", string? name = "Test User")
    {
        var ctrl = new UserController(_repo.Object);
        var claims = new List<Claim>();
        if (sub != null)   claims.Add(new Claim("sub", sub));
        if (email != null) claims.Add(new Claim(EmailClaim, email));
        if (name != null)  claims.Add(new Claim(NameClaim, name));
        ctrl.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity(claims, "test"))
            }
        };
        return ctrl;
    }

    // --- Login ---

    [Fact]
    public async Task Login_MissingSubClaim_ReturnsBadRequest()
    {
        var result = await BuildController(sub: null).Login();
        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task Login_NewUser_CreatesAndReturnsUser()
    {
        _repo.Setup(r => r.GetByAuth0IdAsync("auth0|123")).ReturnsAsync((User?)null);

        User? captured = null;
        _repo.Setup(r => r.AddUserAsync(It.IsAny<User>()))
             .Callback<User>(u => captured = u);

        var result = await BuildController().Login();

        Assert.NotNull(captured);
        Assert.Equal("auth0|123", captured.Auth0Id);
        Assert.Equal("u@test.com", captured.Email);
        _repo.Verify(r => r.SaveAsync(), Times.Once);
        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.IsType<User>(ok.Value);
    }

    [Fact]
    public async Task Login_ExistingUser_ReturnsExistingWithoutSaving()
    {
        var existing = new User { Id = Guid.NewGuid(), Auth0Id = "auth0|123", Email = "u@test.com" };
        _repo.Setup(r => r.GetByAuth0IdAsync("auth0|123")).ReturnsAsync(existing);

        var result = await BuildController().Login();

        _repo.Verify(r => r.AddUserAsync(It.IsAny<User>()), Times.Never);
        _repo.Verify(r => r.SaveAsync(), Times.Never);
        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(existing, ok.Value);
    }

    // --- GetMe ---

    [Fact]
    public async Task GetMe_MissingSubClaim_ReturnsBadRequest()
    {
        var result = await BuildController(sub: null).GetMe();
        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task GetMe_UserFound_ReturnsOk()
    {
        var user = new User { Id = Guid.NewGuid(), Auth0Id = "auth0|123" };
        _repo.Setup(r => r.GetByAuth0IdAsync("auth0|123")).ReturnsAsync(user);

        var result = await BuildController().GetMe();

        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(user, ok.Value);
    }

    [Fact]
    public async Task GetMe_UserNotFound_ReturnsNotFound()
    {
        _repo.Setup(r => r.GetByAuth0IdAsync(It.IsAny<string>())).ReturnsAsync((User?)null);

        var result = await BuildController().GetMe();

        Assert.True(result is NotFoundResult or NotFoundObjectResult);
    }

    // --- GetUserById ---

    [Fact]
    public async Task GetUserById_Found_ReturnsOk()
    {
        var id = Guid.NewGuid();
        var user = new User { Id = id };
        _repo.Setup(r => r.GetUserByIdAsync(id)).ReturnsAsync(user);

        var result = await BuildController().GetUserById(id);

        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(user, ok.Value);
    }

    [Fact]
    public async Task GetUserById_NotFound_ReturnsNotFound()
    {
        _repo.Setup(r => r.GetUserByIdAsync(It.IsAny<Guid>())).ReturnsAsync((User?)null);

        var result = await BuildController().GetUserById(Guid.NewGuid());

        Assert.IsType<NotFoundResult>(result);
    }

    // --- GetAllUsers ---

    [Fact]
    public async Task GetAllUsers_ReturnsOk()
    {
        var users = new List<User> { new() { Id = Guid.NewGuid() } };
        _repo.Setup(r => r.GetAllUsersAsync()).ReturnsAsync(users);

        var result = await BuildController().GetAllUsers();

        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(users, ok.Value);
    }
}
