using Scheduler.BLL.DTOs;
using Scheduler.BLL.Services.Interfaces;
using Scheduler.DAL.Entities;
using Scheduler.DAL.Repositories.Interfaces;

namespace Scheduler.BLL.Services;

public class UserService(IUserRepository userRepository) : IUserService
{
    public async Task<List<User>> GetAllUsersAsync() => await userRepository.GetAllAsync();

    public async Task<User?> CreateUserAsync(CreateUserRequest userRequest)
    {
        var normalizedName = userRequest.Name.Trim().ToLowerInvariant();
        var exists = await userRepository.ExistsByNormalizedNameAsync(normalizedName);
        if (exists) return null;

        var user = new User { Name = userRequest.Name.Trim(), NameNormalized = normalizedName };
        var newUser = await userRepository.AddAsync(user);
        await userRepository.SaveChangesAsync();
        return newUser;
    }

    public async Task<User?> GetUserByIdAsync(int id)
    {
        var user = await userRepository.GetUserByIdAsync(id);
        return user;
    }
}