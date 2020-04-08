using System;
using System.Threading.Tasks;
using Mongo2Go;
using MongoDB.Driver;
using Xunit;

namespace Elastic.Apm.Mongo.IntegrationTests.Fixture
{
    public class MongoFixture<TConfiguration, TDocument> : IAsyncLifetime, IDisposable
        where TConfiguration : IMongoConfiguration<TDocument>, new()
    {
        private readonly TConfiguration _configuration;
        private readonly MongoDbRunner _runner;

        public MongoFixture()
        {
            _configuration = new TConfiguration();
            _runner = MongoDbRunner.Start(additionalMongodArguments: "--setParameter enableTestCommands=1");

            var mongoClient = _configuration.GetMongoClient(_runner.ConnectionString);
            Collection = mongoClient.GetDatabase(_configuration.DatabaseName)
                .GetCollection<TDocument>(_configuration.CollectionName);
        }

        public IMongoCollection<TDocument> Collection { get; }

        public Task InitializeAsync() => _configuration.InitializeAsync(Collection);

        public Task DisposeAsync() => _configuration.DisposeAsync(Collection);

        public void Dispose() => _runner?.Dispose();
    }
}
