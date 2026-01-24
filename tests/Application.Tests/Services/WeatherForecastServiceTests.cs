using Application.Lib.Services;

namespace Application.Tests.Services;

public class WeatherForecastServiceTests
{
    [Fact]
    public void GetForecast_ReturnsDefaultFiveDays()
    {
        var service = new WeatherForecastService();

        var result = service.GetForecast();

        Assert.NotNull(result);
        Assert.Equal(5, result.Count());
    }

    [Theory]
    [InlineData(1)]
    [InlineData(3)]
    [InlineData(7)]
    [InlineData(10)]
    public void GetForecast_ReturnsCorrectNumberOfDays(int days)
    {
        var service = new WeatherForecastService();

        var result = service.GetForecast(days);

        Assert.NotNull(result);
        Assert.Equal(days, result.Count());
    }

    [Fact]
    public void GetForecast_ReturnsFutureDates()
    {
        var service = new WeatherForecastService();
        var today = DateOnly.FromDateTime(DateTime.Now);

        var result = service.GetForecast();

        Assert.All(result, forecast => Assert.True(forecast.Date > today));
    }

    [Fact]
    public void GetForecast_ReturnsValidTemperatureRange()
    {
        var service = new WeatherForecastService();

        var result = service.GetForecast(100);

        Assert.All(result, forecast =>
        {
            Assert.InRange(forecast.TemperatureC, -20, 55);
        });
    }

    [Fact]
    public void GetForecast_ReturnsValidSummary()
    {
        var service = new WeatherForecastService();
        var validSummaries = new[] { "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching" };

        var result = service.GetForecast();

        Assert.All(result, forecast =>
        {
            Assert.Contains(forecast.Summary, validSummaries);
        });
    }

    [Fact]
    public void GetForecast_TemperatureFConversionIsCorrect()
    {
        var service = new WeatherForecastService();

        var result = service.GetForecast();

        Assert.All(result, forecast =>
        {
            var expectedF = 32 + (int)(forecast.TemperatureC / 0.5556);
            Assert.Equal(expectedF, forecast.TemperatureF);
        });
    }
}
