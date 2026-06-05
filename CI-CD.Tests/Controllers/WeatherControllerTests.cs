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
        Assert.NotSame(expected, ok.Value);
        _serviceMock.Verify(s => s.GetForecast(3), Times.Once);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(15)]
    [InlineData(-5)]
    public void Get_WithOutOfRangeDays_ReturnsBadRequest(int days)
    {
        var result = _sut.Get(days);

        Assert.IsType<BadRequestObjectResult>(result.Result);
        _serviceMock.Verify(s => s.GetForecast(It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public void Get_WithNoArgument_DefaultsToFiveDays()
    {
        _serviceMock.Setup(s => s.GetForecast(5)).Returns(Array.Empty<WeatherForecast>());

        _sut.Get();

        _serviceMock.Verify(s => s.GetForecast(5), Times.Once);
    }
}
