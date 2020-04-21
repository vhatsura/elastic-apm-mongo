using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TestEnvironment.Docker;
using TestEnvironment.Docker.Containers.Mongo;

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

            await CreateHostBuilder(args, mongoContainer.GetConnectionString()).Build().RunAsync();

            await dockerEnvironment.Down();
        }

        public static IHostBuilder CreateHostBuilder(string[] args, string mongoConnectionString) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration(ctx =>
                    ctx.AddInMemoryCollection(new Dictionary<string, string> {{"ConnectionString", mongoConnectionString}}))
                .ConfigureWebHostDefaults(webBuilder => { webBuilder.UseStartup<Startup>(); });
    }
}
