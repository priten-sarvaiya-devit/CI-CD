using CI_CD.Models;

namespace CI_CD.Tests.Models;

public class WeatherForecastTests
{
    [Theory]
    [InlineData(0, 32)]
    [InlineData(25, 76)]
    [InlineData(-20, -3)]
    public void TemperatureF_ConvertsFromCelsius(int celsius, int expectedFahrenheit)
    {
        var forecast = new WeatherForecast { TemperatureC = celsius };

        Assert.Equal(expectedFahrenheit, forecast.TemperatureF);
    }
}
