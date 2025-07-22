using Microsoft.EntityFrameworkCore;
using Scheduler.DAL;
using Scheduler.BLL.Services;
using Scheduler.BLL.Services.Interfaces;

namespace Scheduler.Tests.Base;

public abstract class BaseTest : IDisposable
{
    protected readonly SchedulerDbContext Context;
    protected readonly IMeetingService MeetingService;
    protected readonly IUserService UserService;

    protected BaseTest(string? dbName = null)
    {
        var options = new DbContextOptionsBuilder<SchedulerDbContext>()
            .UseInMemoryDatabase(databaseName: dbName ?? Guid.NewGuid().ToString())
            .Options;

        Context = new SchedulerDbContext(options);
        MeetingService = new MeetingService(Context);
        UserService = new UserService(Context);
    }

    public void Dispose()
    {
        Context.Dispose();
    }
}