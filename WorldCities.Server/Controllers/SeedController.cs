using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using System.Security;
using WorldCities.Server.Data;
using WorldCities.Server.Data.Models;

namespace WorldCities.Server.Controllers;
[Route("api/[controller]/[action]")]
[ApiController]
public class SeedController(
    ApplicationDbContext context,
    RoleManager<IdentityRole> roleManager,
    UserManager<ApplicationUser> userManager,
    IWebHostEnvironment env,
    IConfiguration configuration) : ControllerBase
{
    private readonly ApplicationDbContext _context = context;
    private readonly RoleManager<IdentityRole> _roleMangager = roleManager;
    private readonly UserManager<ApplicationUser> _userManager = userManager;
    private readonly IWebHostEnvironment _env = env;
    private readonly IConfiguration _configuration = configuration;

    [HttpGet]
    public async Task<ActionResult> Import()
    {
        // prevents non-development environments from running this method
        if (!_env.IsDevelopment())
        {
            throw new SecurityException("Not allowed");
        }

        var path = Path.Combine(_env.ContentRootPath, "Data/Source/worldcities.xlsx");

        using var stream = System.IO.File.OpenRead(path);
        using var excelPackage = new ExcelPackage(stream);

        // get the first worksheet
        var worksheet = excelPackage.Workbook.Worksheets[0];

        // define how many rows we want to process
        var nEndRow = worksheet.Dimension.End.Row;

        // initialize the record counters
        var numberOfCountriesAdded = 0;
        var numberOfCitiesAdded = 0;

        // create a lookup dictionary
        // containing all the countries already existing
        // in the database (it will be empty on first run).
        var countriesByName = _context.Countries
            .AsNoTracking()
            .ToDictionary(x => x.Name, StringComparer.OrdinalIgnoreCase);

        // iterate through all rows, skipping the first
        for (int nRow = 2; nRow <= nEndRow; nRow++)
        {
            var row = worksheet.Cells[
                nRow, 1, nRow, worksheet.Dimension.End.Column];

            var countryName = row[nRow, 5].GetValue<string>();
            var iso2 = row[nRow, 6].GetValue<string>();
            var iso3 = row[nRow, 7].GetValue<string>();

            // skip this country if it already exists in the database
            if (countriesByName.ContainsKey(countryName))
            {
                continue;
            }

            // create the Country entity and fill it with xlsx data
            var country = new Country
            {
                Name = countryName,
                ISO2 = iso2,
                ISO3 = iso3
            };

            // add the new country to the DB context
            await _context.Countries.AddAsync(country);

            // store the country in our lookup to retrieve its Id later on
            countriesByName.Add(countryName, country);

            // increment the counter
            numberOfCountriesAdded++;
        }

        // save all the countries into the Database
        if (numberOfCountriesAdded > 0)
        {
            await _context.SaveChangesAsync();
        }

        // create a lookup dictionary
        // containing all the cities already existing
        // in the Database (it will be empty on first run).
        var cities = _context.Cities
            .AsNoTracking()
            .ToDictionary(x => (x.Name, x.Lat, x.Lon, x.CountryId));

        // interate through all rows, skipping the first one
        for (int nRow = 2; nRow <= nEndRow; nRow++)
        {
            var row = worksheet.Cells[
                nRow, 1, nRow, worksheet.Dimension.End.Column];

            var name = row[nRow, 1].GetValue<string>();
            var lat = row[nRow, 3].GetValue<decimal>();
            var lon = row[nRow, 4].GetValue<decimal>();
            var countryName = row[nRow, 5].GetValue<string>();

            // retrieve country Id by countryName
            var countryId = countriesByName[countryName].Id;

            // skip this city if it already exists in the database
            if (cities.ContainsKey((name, lat, lon, countryId)))
            {
                continue;
            }

            // create the City entity and fill it with xlsx data
            var city = new City
            {
                Name = name,
                Lat = lat,
                Lon = lon,
                CountryId = countryId
            };

            // add the new city to the DB context
            _context.Cities.Add(city);

            // increment the counter
            numberOfCitiesAdded++;
        }

        // save all the cities into the Database
        if (numberOfCitiesAdded > 0)
        {
            await _context.SaveChangesAsync();
        }

        return new JsonResult(new
        {
            Cities = numberOfCitiesAdded,
            Countries = numberOfCountriesAdded
        });
    }

    [HttpGet]
    public async Task<ActionResult> CreateDefaultUsers()
    {
        // Setup the default role names
        string role_RegisteredUser = "RegisteredUser";
        string role_Administrator = "Administrator";

        // Create the default roles (if they don't already exist)
        if (await _roleMangager.FindByNameAsync(role_RegisteredUser) == null)
        {
            await _roleMangager.CreateAsync(new(role_RegisteredUser));
        }
        if (await _roleMangager.FindByNameAsync(role_Administrator) == null)
        {
            await _roleMangager.CreateAsync(new(role_Administrator));
        }

        // Create a list to track the newly added users
        List<ApplicationUser> addedUserList = [];

        // Check for existence of admin user
        var email_Admin = "admin@email.com";
        if (await _userManager.FindByNameAsync(email_Admin) == null)
        {
            ApplicationUser user_Admin = new()
            {
                SecurityStamp = Guid.NewGuid().ToString(),
                UserName = email_Admin,
                Email = email_Admin
            };

            // Insert the admin user into the database
            await _userManager.CreateAsync(
                user_Admin,
                _configuration["DefaultPasswords:Administrator"]!);

            // Assign the "Registered User" and "Administrator" roles
            await _userManager.AddToRoleAsync(user_Admin, role_RegisteredUser);
            await _userManager.AddToRoleAsync(user_Admin, role_Administrator);

            // Confirm the email and remove lockout
            user_Admin.EmailConfirmed = true;
            user_Admin.LockoutEnabled = false;

            // Add the admin user to the newly added users list
            addedUserList.Add(user_Admin);
        }

        // Check for existence of standard user
        var email_User = "user@email.com";
        if (await _userManager.FindByNameAsync(email_User) == null)
        {
            ApplicationUser user_User = new()
            {
                SecurityStamp = Guid.NewGuid().ToString(),
                UserName = email_User,
                Email = email_User
            };

            // Insert the standard user into the database
            await _userManager.CreateAsync(
                user_User,
                _configuration["DefaultPasswords:RegisteredUser"]!);

            // Assign the "Registered User" role
            await _userManager.AddToRoleAsync(user_User, role_RegisteredUser);

            // Confirm the email and remove lockout
            user_User.EmailConfirmed = true;
            user_User.LockoutEnabled = false;

            // Add the admin user to the newly added users list
            addedUserList.Add(user_User);
        }

        // If newly added users list is not empty, persist changes to database
        if (addedUserList.Count > 0)
        {
            await _context.SaveChangesAsync();
        }

        return new JsonResult(new
        {
            Count = addedUserList.Count,
            Users = addedUserList
        });
    }
}
