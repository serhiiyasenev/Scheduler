using Microsoft.EntityFrameworkCore;
using Scheduler.DAL.Entities;
using Scheduler.DAL.Repositories.Interfaces;

namespace Scheduler.DAL.Repositories;

public class UserRepository(SchedulerDbContext context) : BaseRepository<User>(context), IUserRepository
{
    public Task<List<User>> GetAllAsync()
        => Set.AsNoTracking().ToListAsync();

    public Task<bool> ExistsByNormalizedNameAsync(string normalizedName)
        => Set.AsNoTracking().AnyAsync(u => u.NameNormalized == normalizedName);
}
