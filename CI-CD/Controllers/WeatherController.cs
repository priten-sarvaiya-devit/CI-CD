using CI_CD.Models;
using CI_CD.Services;
using Microsoft.AspNetCore.Mvc;

namespace CI_CD.Controllers;

/// <summary>
/// Exposes weather forecast endpoints.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class WeatherController : ControllerBase
{
    private readonly IWeatherService _weatherService;
    private readonly ILogger<WeatherController> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="WeatherController"/> class.
    /// </summary>
    public WeatherController(
        IWeatherService weatherService,
        ILogger<WeatherController> logger)
    {
        _weatherService = weatherService;
        _logger = logger;
    }

    /// <summary>
    /// Gets a sample weather forecast.
    /// </summary>
    /// <param name="days">Number of days to forecast (1-14). Defaults to 5.</param>
    /// <returns>A collection of weather forecasts.</returns>
    /// <response code="200">Returns the forecast collection.</response>
    /// <response code="400">If <paramref name="days"/> is out of range.</response>
    [HttpGet(Name = "GetWeatherForecast")]
    [ProducesResponseType(typeof(IReadOnlyCollection<WeatherForecast>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public ActionResult<IReadOnlyCollection<WeatherForecast>> Get([FromQuery] int days = 5)
    {
        if (days is < 1 or > 14)
        {
            return BadRequest("The 'days' parameter must be between 1 and 14.");
        }

        _logger.LogInformation("Generating weather forecast for {Days} day(s).", days);
        var forecast = _weatherService.GetForecast(days);
        return Ok(forecast);
    }
}
