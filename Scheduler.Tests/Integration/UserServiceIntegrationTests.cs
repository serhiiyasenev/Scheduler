using Scheduler.BLL.DTOs;
using Scheduler.DAL.Entities;
using Scheduler.Tests.Base;

namespace Scheduler.Tests.Integration;

public class UserServiceIntegrationTests : BaseTest
{
    [Fact]
    public async Task CreateUserAsync_ShouldCreateNewUser_WhenNameIsUnique()
    {
        // Arrange
        var request = new CreateUserRequest("Alice");

        // Act
        var result = await UserService.CreateUserAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(request.Name, result.Name);
        Assert.Single(Context.Users);
    }

    [Fact]
    public async Task GetAllUserAsync_ShouldCreateNewUser_WhenNameIsUnique()
    {
        // Arrange
        var request1 = new CreateUserRequest("Alice");
        var request2 = new CreateUserRequest("Bob");

        await UserService.CreateUserAsync(request1);
        await UserService.CreateUserAsync(request2);

        // Act
        var result = await UserService.GetAllUsersAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
        Assert.Equal(request1.Name, result.First().Name);
        Assert.Equal(request2.Name, result.Last().Name);
    }

    [Fact]
    public async Task CreateUserAsync_ShouldReturnNull_WhenUserWithSameNameExists()
    {
        // Arrange
        Context.Users.Add(new User { Name = "bob", NameNormalized = "bob"});
        await Context.SaveChangesAsync();

        // Act
        var request = new CreateUserRequest("  BoB  ");
        var result = await UserService.CreateUserAsync(request);

        // Assert
        Assert.Null(result);
        Assert.Single(Context.Users);
    }

    [Fact]
    public async Task GetUserById_ShouldReturnUser_WhenUserExists()
    {
        // Arrange
        var request = new CreateUserRequest("AlicE 2");
        var createdUser = await UserService.CreateUserAsync(request);

        // Act
        var result = await UserService.GetUserByIdAsync(createdUser!.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(createdUser.Id, result.Id);
        Assert.Equal("AlicE 2", result.Name);
        Assert.Equal("alice 2", result.NameNormalized);
    }
}
