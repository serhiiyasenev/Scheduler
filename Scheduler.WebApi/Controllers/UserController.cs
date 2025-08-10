using Microsoft.AspNetCore.Mvc;
using Scheduler.BLL.DTOs;
using Scheduler.BLL.Services.Interfaces;
using Scheduler.DAL.Entities;

namespace Scheduler.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UserController(IUserService userService) : ControllerBase
{
    [HttpGet("GetAllUsers")]
    [ProducesResponseType(typeof(List<User>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<User>>> GetAllUsers()
    {
        var users = await userService.GetAllUsersAsync();
        return Ok(users);
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(User), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<User>> GetUserById(int id)
    {
        var user = await userService.GetUserByIdAsync(id);
        return user is null ? NotFound() : Ok(user);
    }

    [HttpPost("CreateUser")]
    [ProducesResponseType(typeof(User), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<User>> CreateUser([FromBody] CreateUserRequest userRequest)
    {
        var user = await userService.CreateUserAsync(userRequest);
        return user == null ? Conflict("User with the same name already exists.") : CreatedAtAction(nameof(GetUserById), new { id = user.Id }, user);
    }
}