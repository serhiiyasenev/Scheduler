using Scheduler.DAL.Entities;
using Scheduler.Tests.Base;

namespace Scheduler.Tests.Integration;

public class UserServiceIntegrationTests : BaseTest
{
    [Fact]
    public async Task CreateUserAsync_ShouldCreateNewUser_WhenNameIsUnique()
    {
        // Arrange
        const string name = "Alice";

        // Act
        var result = await UserService.CreateUserAsync(name);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(name, result!.Name);
        Assert.Single(Context.Users);
    }

    [Fact]
    public async Task GetAllUserAsync_ShouldCreateNewUser_WhenNameIsUnique()
    {
        // Arrange
        const string name1 = "Bob";
        const string name2 = "Alice";

        await UserService.CreateUserAsync(name1);
        await UserService.CreateUserAsync(name2);

        // Act
        var result = await UserService.GetAllUsersAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
        Assert.Equal(name1, result.First().Name);
        Assert.Equal(name2, result.Last().Name);
    }

    [Fact]
    public async Task CreateUserAsync_ShouldReturnNull_WhenUserWithSameNameExists()
    {
        // Arrange
        Context.Users.Add(new User { Name = "bob", NameNormalized = "bob"});
        await Context.SaveChangesAsync();

        // Act
        var result = await UserService.CreateUserAsync("  BoB  ");

        // Assert
        Assert.Null(result);
        Assert.Single(Context.Users);
    }
}
