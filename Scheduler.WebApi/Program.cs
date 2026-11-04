using System.Security.Cryptography;
using System.Text;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.EntityFrameworkCore;
using Scheduler.BLL.DTOs;
using Scheduler.BLL.Services;
using Scheduler.BLL.Services.Interfaces;
using Scheduler.BLL.Validators;
using Scheduler.DAL;
using Scheduler.DAL.Repositories;
using Scheduler.DAL.Repositories.Interfaces;


var builder = WebApplication.CreateBuilder(args);

var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? Array.Empty<string>();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowedOrigins", policy =>
    {
        if (allowedOrigins.Length > 0)
        {
            policy.WithOrigins(allowedOrigins)
                .AllowAnyHeader()
                .AllowAnyMethod();
        }
    });
});

builder.Services.AddControllers(options => { options.Filters.Add<ExceptionFilter>(); });

builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<ScheduleRequestValidator>();

builder.Services.Configure<MeetingSettings>(builder.Configuration.GetSection("MeetingSettings"));
builder.Services.AddDbContext<SchedulerDbContext>(options => options.UseInMemoryDatabase("SchedulerDb"));

builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IMeetingRepository, MeetingRepository>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IMeetingService, MeetingService>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Scheduler API V1");
        c.RoutePrefix = "swagger";
    });
}

app.UseHttpsRedirection();
app.UseCors("AllowedOrigins");

// Protect all API routes until user-based identity and per-user access rules are added.
app.Use(async (context, next) =>
{
    if (!context.Request.Path.StartsWithSegments("/api"))
    {
        await next();
        return;
    }

    var expectedKey = app.Configuration["Security:ApiKey"];
    if (string.IsNullOrWhiteSpace(expectedKey) || Encoding.UTF8.GetByteCount(expectedKey) < 32)
    {
        context.Response.StatusCode = StatusCodes.Status503ServiceUnavailable;
        return;
    }

    if (!context.Request.Headers.TryGetValue("X-Api-Key", out var supplied)
        || supplied.Count != 1 || string.IsNullOrEmpty(supplied[0]))
    {
        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
        return;
    }

    var expectedHash = SHA256.HashData(Encoding.UTF8.GetBytes(expectedKey));
    var providedHash = SHA256.HashData(Encoding.UTF8.GetBytes(supplied[0]));
    if (!CryptographicOperations.FixedTimeEquals(expectedHash, providedHash))
    {
        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
        return;
    }

    await next();
});
app.UseAuthorization();

app.MapControllers();

app.Run();