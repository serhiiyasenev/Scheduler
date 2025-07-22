using Microsoft.AspNetCore.Mvc;
using Scheduler.BLL.Services.Interfaces;
using Scheduler.DAL.Entities;

namespace Scheduler.WebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController(IUserService userService) : ControllerBase
    {
        [HttpGet("GetAllUsers")]
        public async Task<ActionResult<List<User>>> GetAllUsers()
        {
            var users = await userService.GetAllUsersAsync();
            return Ok(users);
        }

        [HttpPost("CreateUser")]
        public async Task<ActionResult<User>> CreateUser([FromBody] string name)
        {
            var user = await userService.CreateUserAsync(name);
            return user == null ? Conflict("User with the same name already exists.") : Ok(user);
        }
    }
}