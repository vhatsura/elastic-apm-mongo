using System;
using MongoDB.Bson.Serialization.Attributes;

namespace Elastic.Apm.Mongo.Examples.AspNetCore
{
    [BsonNoId]
    [BsonIgnoreExtraElements]
    public class WeatherForecast
    {
        public DateTime Date { get; set; }

        public int TemperatureC { get; set; }

        [BsonIgnore]
        public int TemperatureF => 32 + (int) (TemperatureC / 0.5556);

        public string Summary { get; set; }
    }
}
