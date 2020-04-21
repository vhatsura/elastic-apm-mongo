using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using TestEnvironment.Docker;
using TestEnvironment.Docker.Containers.Mongo;

[assembly: ExcludeFromCodeCoverage]

namespace Elastic.Apm.Mongo.Examples.AspNetCore
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            await using var dockerEnvironment = new DockerEnvironmentBuilder()
                .AddMongoContainer("mongo")
                .Build();

            await dockerEnvironment.Up();
            var mongoContainer = dockerEnvironment.GetContainer<MongoContainer>("mongo");

            var host = CreateHostBuilder(args, mongoContainer.GetConnectionString()).Build();

            await InitializeDatabaseWithData(host);

            await host.RunAsync();

            await dockerEnvironment.Down();
        }

        private static async Task InitializeDatabaseWithData(IHost host)
        {
            using var scope = host.Services.CreateScope();
            var services = scope.ServiceProvider;

            try
            {
                var mongoClient = services.GetRequiredService<IMongoClient>();
                var collection = mongoClient.GetDatabase("local").GetCollection<WeatherForecast>("documents");

                var summaries = new[]
                {
                    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering",
                    "Scorching"
                };

                var rng = new Random();
                var forecasts = Enumerable.Range(1, 5).Select(index => new WeatherForecast
                    {
                        Date = DateTime.Now.AddDays(index),
                        TemperatureC = rng.Next(-20, 55),
                        Summary = summaries[rng.Next(summaries.Length)]
                    })
                    .ToArray();

                await collection.InsertManyAsync(forecasts);
            }
            catch (Exception ex)
            {
                var logger = services.GetRequiredService<ILogger<Program>>();
                logger.LogError(ex, "An error occurred while seeding the database.");
            }
        }

        public static IHostBuilder CreateHostBuilder(string[] args, string mongoConnectionString) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration(ctx =>
                    ctx.AddInMemoryCollection(new Dictionary<string, string>
                    {
                        {"MongoConnectionString", mongoConnectionString}
                    }))
                .ConfigureWebHostDefaults(webBuilder => { webBuilder.UseStartup<Startup>(); });
    }
}
