using CI_CD.Models;

namespace CI_CD.Services;

/// <summary>
/// Defines the contract for retrieving weather forecast data.
/// </summary>
public interface IWeatherService
{
    /// <summary>
    /// Returns a collection of weather forecasts for the upcoming days.
    /// </summary>
    /// <param name="days">The number of days to forecast. Defaults to 5.</param>
    /// <returns>A read-only collection of <see cref="WeatherForecast"/> items.</returns>
    IReadOnlyCollection<WeatherForecast> GetForecast(int days = 5);
}
