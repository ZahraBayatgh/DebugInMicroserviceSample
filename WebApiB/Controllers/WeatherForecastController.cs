using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace WebApiB.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private readonly ILogger<WeatherForecastController> _logger;

        public WeatherForecastController(ILogger<WeatherForecastController> logger)
        {
            _logger = logger;
        }

        [HttpGet(Name = "GetWeatherForecast")]
        public async Task<IActionResult> Get()
        {
            var _httpClient = new HttpClient();

            var traceId = Activity.Current.TraceId;
            var spanId = Activity.Current.SpanId;
            var parentSpanId = Activity.Current.ParentSpanId;

            _logger.LogInformation("The GetWeatherForecast is calling!(WebApiB)");

            var result = await _httpClient.GetStringAsync(
                $"http://localhost:5002/WeatherForecast");

            return Ok(result);
        }
    }
}