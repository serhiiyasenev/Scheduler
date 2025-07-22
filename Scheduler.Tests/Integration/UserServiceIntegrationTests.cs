using Scheduler.DAL.Entities;
using Scheduler.Tests.Base;

namespace Scheduler.Tests.Integration;

public class UserServiceIntegrationTests : BaseTest
{
    [Fact]
    public async Task CreateUserAsync_ShouldCreateNewUser_WhenNameIsUnique()
    {
        // Arrange
        var name = "Alice";

        // Act
        var result = await UserService.CreateUserAsync(name);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(name, result!.Name);
        Assert.Single(Context.Users);
    }

    [Fact]
    public async Task CreateUserAsync_ShouldReturnNull_WhenUserWithSameNameExists()
    {
        // Arrange
        Context.Users.Add(new User { Name = "bob" });
        await Context.SaveChangesAsync();

        // Act
        var result = await UserService.CreateUserAsync("  BoB  ");

        // Assert
        Assert.Null(result);
        Assert.Single(Context.Users);
    }
}
