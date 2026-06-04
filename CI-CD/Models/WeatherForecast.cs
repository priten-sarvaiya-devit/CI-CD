namespace CI_CD.Models;

/// <summary>
/// Represents a single day's weather forecast.
/// </summary>
public class WeatherForecast
{
    /// <summary>The date of the forecast.</summary>
    public DateOnly Date { get; set; }

    /// <summary>The temperature in degrees Celsius.</summary>
    public int TemperatureC { get; set; }

    /// <summary>The temperature in degrees Fahrenheit, derived from Celsius.</summary>
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);

    /// <summary>A short textual summary of the conditions (e.g. "Mild").</summary>
    public string? Summary { get; set; }
}
