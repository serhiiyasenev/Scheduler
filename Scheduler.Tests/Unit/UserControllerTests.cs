using Microsoft.AspNetCore.Mvc;
using Moq;
using Scheduler.BLL.DTOs;
using Scheduler.BLL.Services.Interfaces;
using Scheduler.DAL.Entities;
using Scheduler.WebApi.Controllers;

namespace Scheduler.Tests.Unit;

public class UserControllerTests
{
    private readonly Mock<IUserService> _mockService;
    private readonly UserController _controller;

    public UserControllerTests()
    {
        _mockService = new Mock<IUserService>();
        _controller = new UserController(_mockService.Object);
    }

    [Fact]
    public async Task CreateUser_ShouldReturnOk_WhenUserCreated()
    {
        var expectedUser = new User { Id = 1, Name = "Alice" };

        _mockService.Setup(s => s.CreateUserAsync(new CreateUserRequest("Alice")))
            .ReturnsAsync(expectedUser);

        var result = await _controller.CreateUser(new CreateUserRequest("Alice"));

        var okResult = Assert.IsType<CreatedAtActionResult>(result.Result);
        var user = Assert.IsType<User>(okResult.Value);
        Assert.Equal(expectedUser.Id, user.Id);
        Assert.Equal(expectedUser.Name, user.Name);
    }

    [Fact]
    public async Task GetAllUsers_ShouldReturnUserList()
    {
        // Arrange
        var users = new List<User>
        {
            new() { Id = 1, Name = "Alice" },
            new() { Id = 2, Name = "Bob" }
        };

        _mockService.Setup(s => s.GetAllUsersAsync())
            .ReturnsAsync(users);

        // Act
        var result = await _controller.GetAllUsers();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnedUsers = Assert.IsAssignableFrom<List<User>>(okResult.Value);
        Assert.Equal(2, returnedUsers.Count);
        Assert.Equal("Alice", returnedUsers[0].Name);
        Assert.Equal("Bob", returnedUsers[1].Name);
    }

    [Fact]
    public async Task GetUsersById_ShouldReturnUser()
    {
        // Arrange
        var user = new User { Id = 1, Name = "Alice" };

        _mockService.Setup(s => s.GetUserByIdAsync(1))
            .ReturnsAsync(user);

        // Act
        var result = await _controller.GetUserById(1);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnedUser = Assert.IsAssignableFrom<User>(okResult.Value);
        Assert.Equal("Alice", returnedUser.Name);
    }
}