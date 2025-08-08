using Microsoft.EntityFrameworkCore;
using Scheduler.DAL.Repositories.Interfaces;

namespace Scheduler.DAL.Repositories;

public abstract class BaseRepository<T>(SchedulerDbContext context) : IRepository<T> where T : class
{
    protected readonly SchedulerDbContext Context = context;
    protected readonly DbSet<T> Set = context.Set<T>();

    public virtual async Task<T> AddAsync(T entity)
    {
        var entry = await Set.AddAsync(entity);
        await Context.SaveChangesAsync();
        return entry.Entity;
    }
}
