using Scheduler.DAL.Entities;

namespace Scheduler.DAL.Repositories.Interfaces;

public interface IUserRepository : IRepository<User>
{
    Task<List<User>> GetAllAsync();
    Task<bool> ExistsByNormalizedNameAsync(string normalizedName);
    Task<List<int>> GetExistingUserIdsAsync(IEnumerable<int> ids);
    Task<User?> GetUserByIdAsync(int id);
}
