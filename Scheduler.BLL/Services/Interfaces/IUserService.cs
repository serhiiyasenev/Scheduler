using Scheduler.DAL.Entities;

namespace Scheduler.BLL.Services.Interfaces;

public interface IUserService
{
    Task<List<User>> GetAllUsersAsync();
    Task<User?> CreateUserAsync(string name);
}