using Microsoft.EntityFrameworkCore;
using Scheduler.DAL;
using Scheduler.DAL.Entities;
using Scheduler.BLL.Services.Interfaces;

namespace Scheduler.BLL.Services
{
    public class UserService(SchedulerDbContext context) : IUserService
    {
        public async Task<List<User>> GetAllUsersAsync()
        {
            return await context.Users.AsNoTracking().ToListAsync();
        }

        public async Task<User?> CreateUserAsync(string name)
        {
            var normalizedName = name.Trim().ToLower();
            var exists = await context.Users.AnyAsync(u => u.Name == normalizedName);
            if (exists)
                return null;

            var user = new User { Name = name };
            context.Users.Add(user);
            await context.SaveChangesAsync();
            return user;
        }
    }
}