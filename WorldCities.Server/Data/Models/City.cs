namespace WorldCities.Server.Data.Models;

public class City
{
    #region Properties
    /// <summary>
    /// The unique id and primary key for this City
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// City name (in UTF8 format)
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    /// City latitude
    /// </summary>
    public decimal Lat { get; set; }

    /// <summary>
    /// City longitude
    /// </summary>
    public decimal Lon { get; set; }

    /// <summary>
    /// Country Id (foreign key)
    /// </summary>
    public int CountryId { get; set; }
    #endregion

    #region Navigation Properties
    /// <summary>
    /// The country related to this city
    /// </summary>
    public Country? Country { get; set; }
    #endregion
}
