using Microsoft.EntityFrameworkCore;
using Scheduler.DAL.Entities;
using Scheduler.DAL.Repositories.Interfaces;

namespace Scheduler.DAL.Repositories;

public class UserRepository(SchedulerDbContext context) : BaseRepository<User>(context), IUserRepository
{
    public async Task<List<User>> GetAllAsync()
        => await Set.AsNoTracking().ToListAsync();

    public async Task<bool> ExistsByNormalizedNameAsync(string normalizedName)
        => await Set.AsNoTracking().AnyAsync(u => u.NameNormalized == normalizedName);

    public async Task<List<int>> GetExistingUserIdsAsync(IEnumerable<int> ids)
    {
        var distinctIds = ids.Distinct().ToList();
        return await Set
            .Where(u => distinctIds.Contains(u.Id))
            .Select(u => u.Id)
            .ToListAsync();
    }

    public async Task<User?> GetUserByIdAsync(int id)
    {
        return await Set.FindAsync(id);
    }
}
