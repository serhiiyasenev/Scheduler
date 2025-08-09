using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Scheduler.BLL.DTOs;
using Scheduler.BLL.Services;
using Scheduler.BLL.Services.Interfaces;
using Scheduler.DAL;
using Scheduler.DAL.Repositories;
using Scheduler.DAL.Repositories.Interfaces;

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

        var meetingSettings = Options.Create(new MeetingSettings
        {
            BaseUrl = "https://localhost:7272"
        });

        Context = new SchedulerDbContext(options);
        MeetingRepository = new MeetingRepository(Context);
        UserRepository = new UserRepository(Context);
        MeetingService = new MeetingService(MeetingRepository, UserRepository, meetingSettings);
        UserService = new UserService(UserRepository);
    }

    public void Dispose()
    {
        Context.Dispose();
    }
}