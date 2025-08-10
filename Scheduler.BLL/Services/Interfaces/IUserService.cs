using Scheduler.BLL.DTOs;
using Scheduler.DAL.Entities;

namespace Scheduler.BLL.Services.Interfaces;

public interface IUserService
{
    Task<List<User>> GetAllUsersAsync();
    Task<User?> CreateUserAsync(CreateUserRequest user);
    Task<User?> GetUserByIdAsync(int id);
}