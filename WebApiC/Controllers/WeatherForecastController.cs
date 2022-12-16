using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace WebApiC.Controllers
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

            _logger.LogInformation("The GetWeatherForecast is calling!(WebApiC)");

            return Ok();
        }
    }
}