using Microsoft.EntityFrameworkCore;
using Scheduler.DAL.Entities;

namespace Scheduler.DAL
{
    public class SchedulerDbContext(DbContextOptions<SchedulerDbContext> options) : DbContext(options)
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Meeting> Meetings { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<MeetingParticipant>().HasKey(mp => new { mp.MeetingId, mp.UserId });
        }
    }
}
