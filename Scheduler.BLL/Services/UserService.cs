using Scheduler.DAL.Repositories.Interfaces;
using Scheduler.DAL.Entities;
using Scheduler.BLL.Services.Interfaces;

namespace Scheduler.BLL.Services
{
    public class UserService(IUserRepository userRepository) : IUserService
    {
        public Task<List<User>> GetAllUsersAsync()
            => userRepository.GetAllAsync();

        public async Task<User?> CreateUserAsync(string name)
        {
            var normalizedName = name.Trim().ToLowerInvariant();
            var exists = await userRepository.ExistsByNormalizedNameAsync(normalizedName);
            if (exists) return null;

            var user = new User { Name = name.Trim(), NameNormalized = normalizedName };
            await userRepository.AddAsync(user);
            await userRepository.SaveChangesAsync();
            return user;
        }
    }
}
