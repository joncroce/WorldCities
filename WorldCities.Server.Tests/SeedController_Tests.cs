using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Moq;
using WorldCities.Server.Controllers;
using WorldCities.Server.Data;
using WorldCities.Server.Data.Models;

namespace WorldCities.Server.Tests;
public class SeedController_Tests
{
    /// <summary>
    /// Test the CreateDefaultUsers method
    /// </summary>
    [Fact]
    public async Task CreateDefaultUsers()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: "WorldCities")
            .Options;
        var mockEnv = Mock.Of<IWebHostEnvironment>();
        var mockConfiguration = new Mock<IConfiguration>();
        mockConfiguration
            .SetupGet(x => x[It.Is<string>(s => s == "DefaultPasswords:RegisteredUser")])
            .Returns("M0ckPa$$word");
        mockConfiguration
            .SetupGet(x => x[It.Is<string>(s => s == "DefaultPasswords:Administrator")])
            .Returns("M0ckPa$$word");

        using var context = new ApplicationDbContext(options);
        var roleManager = IdentityHelper.GetRoleManager(
            new RoleStore<IdentityRole>(context));
        var userManager = IdentityHelper.GetUserManager(
            new UserStore<ApplicationUser>(context));
        var controller = new SeedController(
            context,
            roleManager,
            userManager,
            mockEnv,
            mockConfiguration.Object);

        ApplicationUser user_Admin = null!;
        ApplicationUser user_User = null!;
        ApplicationUser user_NotExisting = null!;

        // Act
        await controller.CreateDefaultUsers();

        user_Admin = await userManager.FindByEmailAsync("admin@email.com");
        user_User = await userManager.FindByEmailAsync("user@email.com");
        user_NotExisting = await userManager.FindByEmailAsync("notexisting@email.com");

        // Assert
        Assert.NotNull(user_Admin);
        Assert.NotNull(user_User);
        Assert.Null(user_NotExisting);
    }
}
