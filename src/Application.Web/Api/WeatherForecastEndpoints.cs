using Application.Lib.Services;

namespace Application.Web.Api;

public static class WeatherForecastEndpoints
{
    public static void AddWeatherForecastEndpoints(this RouteGroupBuilder group)
    {
        group.MapGet("/weatherforecast", (WeatherForecastService service) =>
        {
            var forecast = service.GetForecast();
            return Results.Ok(forecast);
        })
        .WithName("GetWeatherForecast");
    }
}
