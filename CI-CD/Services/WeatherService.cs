using CI_CD.Models;

namespace CI_CD.Services;

/// <summary>
/// Default implementation of <see cref="IWeatherService"/> that produces
/// deterministic-shaped sample weather data.
/// </summary>
public class WeatherService : IWeatherService
{
    private static readonly string[] Summaries =
    [
        "Freezing", "Bracing", "Chilly", "Cool", "Mild",
        "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    ];

    /// <inheritdoc />
    public IReadOnlyCollection<WeatherForecast> GetForecast(int days = 5)
    {
        if (days < 1)
        {
            throw new ArgumentOutOfRangeException(
                nameof(days), days, "The number of days must be at least 1.");
        }

        return Enumerable.Range(1, days)
            .Select(index => new WeatherForecast
            {
                Date = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(index)),
                TemperatureC = Random.Shared.Next(-20, 55),
                Summary = Summaries[Random.Shared.Next(Summaries.Length)]
            })
            .ToArray();
    }
}
