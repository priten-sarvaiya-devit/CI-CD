using CI_CD.Models;
using CI_CD.Services;

namespace CI_CD.Tests.Services;

public class WeatherServiceTests
{
    private readonly IWeatherService _sut = new WeatherService();

    [Fact]
    public void GetForecast_WithDefaultArgs_ReturnsFiveForecasts()
    {
        var result = _sut.GetForecast();

        Assert.Equal(5, result.Count);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(3)]
    [InlineData(7)]
    [InlineData(14)]
    public void GetForecast_WithGivenDays_ReturnsThatManyForecasts(int days)
    {
        var result = _sut.GetForecast(days);

        Assert.Equal(days, result.Count);
    }

    [Fact]
    public void GetForecast_ProducesConsecutiveFutureDates()
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        var result = _sut.GetForecast(5).ToList();

        for (var i = 0; i < result.Count; i++)
        {
            Assert.Equal(today.AddDays(i + 1), result[i].Date);
        }
    }

    [Fact]
    public void GetForecast_TemperatureIsWithinExpectedRange()
    {
        var result = _sut.GetForecast(10);

        Assert.All(result, f => Assert.InRange(f.TemperatureC, -20, 54));
    }

    [Fact]
    public void GetForecast_EverySummaryIsPopulated()
    {
        var result = _sut.GetForecast(10);

        Assert.All(result, f => Assert.False(string.IsNullOrWhiteSpace(f.Summary)));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    public void GetForecast_WithNonPositiveDays_Throws(int days)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => _sut.GetForecast(days));
    }
}
