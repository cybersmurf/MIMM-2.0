using Application.Lib.Models;

namespace Application.Tests.Models;

public class WeatherForecastDtoTests
{
    [Fact]
    public void WeatherForecastDto_InitializesCorrectly()
    {
        var date = new DateOnly(2026, 1, 15);
        var temperatureC = 20;
        var summary = "Warm";

        var dto = new WeatherForecastDto(date, temperatureC, summary);

        Assert.Equal(date, dto.Date);
        Assert.Equal(temperatureC, dto.TemperatureC);
        Assert.Equal(summary, dto.Summary);
    }

    [Theory]
    [InlineData(0, 32)]
    [InlineData(10, 49)]
    [InlineData(20, 67)]
    [InlineData(-10, 15)]
    [InlineData(30, 85)]
    public void TemperatureF_ConvertsCorrectly(int temperatureC, int expectedF)
    {
        var dto = new WeatherForecastDto(DateOnly.MinValue, temperatureC, "Test");

        Assert.Equal(expectedF, dto.TemperatureF);
    }

    [Fact]
    public void WeatherForecastDto_SupportsRecordEquality()
    {
        var date = new DateOnly(2026, 1, 15);
        var dto1 = new WeatherForecastDto(date, 20, "Warm");
        var dto2 = new WeatherForecastDto(date, 20, "Warm");

        Assert.Equal(dto1, dto2);
    }

    [Fact]
    public void WeatherForecastDto_DifferentValuesAreNotEqual()
    {
        var date = new DateOnly(2026, 1, 15);
        var dto1 = new WeatherForecastDto(date, 20, "Warm");
        var dto2 = new WeatherForecastDto(date, 21, "Warm");

        Assert.NotEqual(dto1, dto2);
    }
}
