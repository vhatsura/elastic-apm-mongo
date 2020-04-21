using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;

namespace Elastic.Apm.Mongo.Examples.AspNetCore.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private readonly ILogger<WeatherForecastController> _logger;
        private readonly IMongoCollection<WeatherForecast> _documentsCollection;

        public WeatherForecastController(IMongoClient mongoClient, ILogger<WeatherForecastController> logger)
        {
            _logger = logger;
            _documentsCollection = mongoClient.GetDatabase("local").GetCollection<WeatherForecast>("documents");
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var forecasts = await _documentsCollection.Find(Builders<WeatherForecast>.Filter.Empty)
                .ToListAsync();

            return Ok(forecasts);
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] WeatherForecast forecast)
        {
            await _documentsCollection.InsertOneAsync(forecast);
            return Ok();
        }
    }
}
