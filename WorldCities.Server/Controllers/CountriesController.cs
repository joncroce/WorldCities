using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq.Dynamic.Core;
using WorldCities.Server.Data;
using WorldCities.Server.Data.Models;

namespace WorldCities.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CountriesController(ApplicationDbContext context) : ControllerBase
    {
        private readonly ApplicationDbContext _context = context;

        // GET: api/Countries
        // Get: api/Countries/?pageIndex=0&pageSize=10
        [HttpGet]
        public async Task<ActionResult<ApiResult<CountryDto>>> GetCountries(
            int pageIndex = 0,
            int pageSize = 10,
            string? sortColumn = null,
            string? sortOrder = null,
            string? filterColumn = null,
            string? filterQuery = null)
        {
            return await ApiResult<CountryDto>.CreateAsync(
                _context.Countries.AsNoTracking()
                    .Select(c => new CountryDto()
                    {
                        Id = c.Id,
                        Name = c.Name,
                        ISO2 = c.ISO2,
                        ISO3 = c.ISO3,
                        TotalCities = c.Cities!.Count
                    }),
                pageIndex,
                pageSize,
                sortColumn,
                sortOrder,
                filterColumn,
                filterQuery);
        }

        // GET: api/Countries/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Country>> GetCountry(int id)
        {
            var country = await _context.Countries.FindAsync(id);

            if (country == null)
            {
                return NotFound();
            }

            return country;
        }

        // PUT: api/Countries/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        [Authorize(Roles = "RegisteredUser")]
        public async Task<IActionResult> PutCountry(int id, Country country)
        {
            if (id != country.Id)
            {
                return BadRequest();
            }

            _context.Entry(country).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CountryExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/Countries
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        [Authorize(Roles = "RegisteredUser")]
        public async Task<ActionResult<Country>> PostCountry(Country country)
        {
            _context.Countries.Add(country);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetCountry", new { id = country.Id }, country);
        }

        // DELETE: api/Countries/5
        [HttpDelete("{id}")]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> DeleteCountry(int id)
        {
            var country = await _context.Countries.FindAsync(id);
            if (country == null)
            {
                return NotFound();
            }

            _context.Countries.Remove(country);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool CountryExists(int id)
        {
            return _context.Countries.Any(e => e.Id == id);
        }

        [HttpPost]
        [Route("IsDupeField")]
        public bool IsDupeField(
            int countryId,
            string fieldName,
            string fieldValue)
        {
            // Default approach using strongly-typed lambda expressions
            //return fieldName switch
            //{
            //    "name" => _context.Countries.Any(
            //                            c => c.Name == fieldValue && c.Id != countryId),
            //    "iso2" => _context.Countries.Any(
            //                            c => c.ISO2 == fieldValue && c.Id != countryId),
            //    "iso3" => _context.Countries.Any(
            //                            c => c.ISO3 == fieldValue && c.Id != countryId),
            //    _ => false
            //};

            // Alternative approach using System.Linq.Dynamic.Core
            // Note: May come with minor performance penalty due
            // to additional overhead of using library.
            return ApiResult<Country>.IsValidProperty(fieldName, true)
                && _context.Countries.Any(
                    string.Format("{0} == @0 && Id != @1", fieldName),
                    fieldValue,
                    countryId);
        }
    }
}
