using CI_CD.Controllers;
using CI_CD.Models;
using CI_CD.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace CI_CD.Tests.Controllers;

public class WeatherControllerTests
{
    private readonly Mock<IWeatherService> _serviceMock = new();
    private readonly WeatherController _sut;

    public WeatherControllerTests()
    {
        _sut = new WeatherController(
            _serviceMock.Object,
            NullLogger<WeatherController>.Instance);
    }

    [Fact]
    public void Get_WithValidDays_ReturnsOkWithServiceData()
    {
        var expected = new[]
        {
            new WeatherForecast { Date = DateOnly.FromDateTime(DateTime.UtcNow), TemperatureC = 21, Summary = "Mild" }
        };
        _serviceMock.Setup(s => s.GetForecast(3)).Returns(expected);

        var result = _sut.Get(3);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Same(expected, ok.Value);
        _serviceMock.Verify(s => s.GetForecast(3), Times.Once);
    }
}
