using AirReservationsApp.Controllers;
using AirReservationsApp.Data;
using AirReservationsApp.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.EntityFrameworkCore;
using Moq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

public class UserControllerTests
{
    private readonly Mock<UserManager<User>> _userManagerMock;
    private readonly Mock<SignInManager<User>> _signInManagerMock;
    private readonly Mock<RoleManager<IdentityRole>> _roleManagerMock;
    private readonly ApplicationDbContext _dbContext;
    private readonly UserController _controller;

    public UserControllerTests()
    {
        // Configure in-memory database
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: "TestDb")
            .Options;
        _dbContext = new ApplicationDbContext(options);

        _userManagerMock = new Mock<UserManager<User>>(
            Mock.Of<IUserStore<User>>(), null, null, null, null, null, null, null, null);

        _signInManagerMock = new Mock<SignInManager<User>>(
            _userManagerMock.Object,
            Mock.Of<IHttpContextAccessor>(),
            Mock.Of<IUserClaimsPrincipalFactory<User>>(),
            null, null, null, null);

        _roleManagerMock = new Mock<RoleManager<IdentityRole>>(
            Mock.Of<IRoleStore<IdentityRole>>(), null, null, null, null);

        _controller = new UserController(
            _userManagerMock.Object,
            _signInManagerMock.Object,
            _roleManagerMock.Object,
            _dbContext);
    }

    [Fact]
    public async Task Login_WithValidCredentials_ShouldRedirectToCorrectRole()
    {
        // Arrange
        var user = new User { UserName = "testUser", UserType = "Admin" };
        var model = new LoginViewModel { UserName = "testUser", Password = "ValidPassword", RememberMe = false };

        _userManagerMock.Setup(u => u.FindByNameAsync(model.UserName)).ReturnsAsync(user);
        _signInManagerMock.Setup(s => s.PasswordSignInAsync(model.UserName, model.Password, model.RememberMe, false))
            .ReturnsAsync(Microsoft.AspNetCore.Identity.SignInResult.Success);
        _userManagerMock.Setup(u => u.GetRolesAsync(user)).ReturnsAsync(new List<string> { "Admin" });

        // Act
        var result = await _controller.Login(model) as RedirectToActionResult;

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Index", result.ActionName);
        Assert.Equal("Admin", result.ControllerName);
    }

    [Fact]
    public async Task Login_WithInvalidCredentials_ShouldReturnViewWithError()
    {
        // Arrange
        var model = new LoginViewModel { UserName = "testUser", Password = "WrongPassword", RememberMe = false };
        var user = new User { UserName = "testUser", UserType="Viewer" };

        _userManagerMock.Setup(u => u.FindByNameAsync(model.UserName)).ReturnsAsync(user);
        _signInManagerMock.Setup(s => s.PasswordSignInAsync(model.UserName, model.Password, model.RememberMe, false))
            .ReturnsAsync(Microsoft.AspNetCore.Identity.SignInResult.Failed);

        _controller.TempData = new TempDataDictionary(new DefaultHttpContext(), Mock.Of<ITempDataProvider>()); // Fix TempData issue
        // Act
        var result = await _controller.Login(model) as ViewResult;

        // Assert
        Assert.NotNull(result);
        Assert.False(_controller.ModelState.IsValid);
    }

    [Fact]
    public async Task Login_WithNonExistentUser_ShouldReturnViewWithError()
    {
        // Arrange
        var model = new LoginViewModel { UserName = "nonexistentUser", Password = "password", RememberMe = false };

        _userManagerMock.Setup(u => u.FindByNameAsync(model.UserName)).ReturnsAsync((User?)null);
        _controller.TempData = new TempDataDictionary(new DefaultHttpContext(), Mock.Of<ITempDataProvider>()); // Fix TempData issue

        // Act
        var result = await _controller.Login(model) as ViewResult;

        // Assert
        Assert.NotNull(result);
        Assert.False(_controller.ModelState.IsValid);
    }
}
