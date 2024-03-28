using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;

namespace WorldCities.Server.Tests;
public static class IdentityHelper
{
    public static RoleManager<TIdentityRole> GetRoleManager<TIdentityRole>(
        IRoleStore<TIdentityRole> roleStore)
        where TIdentityRole : IdentityRole
    {
        return new RoleManager<TIdentityRole>(
            roleStore,
            Array.Empty<IRoleValidator<TIdentityRole>>(),
            new UpperInvariantLookupNormalizer(),
            new Mock<IdentityErrorDescriber>().Object,
            new Mock<ILogger<RoleManager<TIdentityRole>>>().Object);
    }

    public static UserManager<TIdentityUser> GetUserManager<TIdentityUser>(
        IUserStore<TIdentityUser> userStore)
        where TIdentityUser : IdentityUser
    {
        return new UserManager<TIdentityUser>(
            userStore,
            new Mock<IOptions<IdentityOptions>>().Object,
            new Mock<IPasswordHasher<TIdentityUser>>().Object,
            Array.Empty<IUserValidator<TIdentityUser>>(),
            Array.Empty<IPasswordValidator<TIdentityUser>>(),
            new UpperInvariantLookupNormalizer(),
            new Mock<IdentityErrorDescriber>().Object,
            new Mock<IServiceProvider>().Object,
            new Mock<ILogger<UserManager<TIdentityUser>>>().Object);
    }
}
