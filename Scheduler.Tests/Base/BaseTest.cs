using Microsoft.EntityFrameworkCore;
using Scheduler.DAL;
using Scheduler.DAL.Repositories;
using Scheduler.DAL.Repositories.Interfaces;
using Scheduler.BLL.Services;
using Scheduler.BLL.Services.Interfaces;

namespace Scheduler.Tests.Base;

public abstract class BaseTest : IDisposable
{
    protected readonly SchedulerDbContext Context;
    protected readonly IMeetingService MeetingService;
    protected readonly IUserService UserService;
    protected readonly IMeetingRepository MeetingRepository;
    protected readonly IUserRepository UserRepository;

    protected BaseTest(string? dbName = null)
    {
        var options = new DbContextOptionsBuilder<SchedulerDbContext>()
            .UseInMemoryDatabase(databaseName: dbName ?? Guid.NewGuid().ToString())
            .Options;

        Context = new SchedulerDbContext(options);
        MeetingRepository = new MeetingRepository(Context);
        UserRepository = new UserRepository(Context);
        MeetingService = new MeetingService(MeetingRepository);
        UserService = new UserService(UserRepository);
    }

    public void Dispose()
    {
        Context.Dispose();
    }
}