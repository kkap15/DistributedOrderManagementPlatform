using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UserService.Helpers;
using UserService.Models;
using UserService.Repositories;

namespace UserService.Controllers;

[ApiController]
[Route("api/user")]
[Authorize]
public class UserController(IUserRepositories userRepositories) : ControllerBase
{   
    [HttpPost("login")]
    public async Task<IActionResult> Login()
    {
        var userClaims = new UserClaims
        {
            Auth0IdClaim = User.FindFirst("sub")?.Value,
            EmailClaim = User.FindFirst("http://microservices-api:email")?.Value,
            NameClaim = User.FindFirst("http://microservices-api:name")?.Value
        };
        
        if (string.IsNullOrEmpty(userClaims.Auth0IdClaim))
        {
            return BadRequest(new
            {
                message = "Missing Auth0 identity"
            });
        }

        var user = await userRepositories.GetByAuth0IdAsync(userClaims.Auth0IdClaim);
        if (user == null)
        {
            user = await UserHelper.CreateNewUser(userClaims);
            await userRepositories.AddUserAsync(user);
            await userRepositories.SaveAsync();
        } 
        
        return Ok(user);
    }
    
    [HttpGet("me")]
    public async Task<IActionResult> GetMe()
    {
        // prefer JWT sub claim (direct call), fall back to header (Gateway call)
        var auth0Id = User.FindFirst("sub")?.Value;

        if (string.IsNullOrEmpty(auth0Id))
        {
            return BadRequest(new
            {
                message = "Missing Auth0 identity — provide a Bearer token header"
            });   
        }

        var user = await userRepositories.GetByAuth0IdAsync(auth0Id);
        return user is null 
            ? NotFound( new { message = "User Not Found in Database" }) 
            : Ok(user);
    }
    
    [HttpGet("{id}")]
    public async Task<IActionResult> GetUserById(Guid id)
    {
        var user = await userRepositories.GetUserByIdAsync(id);
        if (user == null)
        {
            return NotFound();
        }
        return user is null 
            ? NotFound(new { message = "User Not Found in Database" }) 
            : Ok(user);
    }
    
    [HttpGet("users")]
    public async Task<IActionResult> GetAllUsers()
    {
        var users = await userRepositories.GetAllUsersAsync();
        return Ok(users);
    }
}