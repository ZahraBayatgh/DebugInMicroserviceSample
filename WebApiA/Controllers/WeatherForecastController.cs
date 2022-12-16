using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace WebApiA.Controllers
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
            var traceId = Activity.Current.TraceId;
            var spanId = Activity.Current.SpanId;
            var parentSpanId = Activity.Current.ParentSpanId;

            var _httpClient = new HttpClient();
            _logger.LogInformation("The GetWeatherForecast is calling!(WebApiA)");

            var result = await _httpClient.GetStringAsync(
                $"http://localhost:5001/WeatherForecast");

            return Ok(result);
        }
    }
}