using Microsoft.EntityFrameworkCore;
using Scheduler.DAL;
using Scheduler.BLL.Services;
using Scheduler.BLL.Services.Interfaces;

namespace Scheduler.Tests;

public abstract class BaseTest : IDisposable
{
    protected readonly SchedulerDbContext Context;
    protected readonly IMeetingService Service;

    protected BaseTest(string? dbName = null)
    {
        var options = new DbContextOptionsBuilder<SchedulerDbContext>()
            .UseInMemoryDatabase(databaseName: dbName ?? Guid.NewGuid().ToString())
            .Options;

        Context = new SchedulerDbContext(options);
        Service = new MeetingService(Context);
    }

    public void Dispose()
    {
        Context.Dispose();
    }
}