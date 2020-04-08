using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Elastic.Apm.Api;
using Elastic.Apm.Mongo.IntegrationTests.Fixture;
using Elastic.Apm.Mongo.IntegrationTests.Mocks;
using MongoDB.Bson;
using MongoDB.Driver;
using Xunit;

namespace Elastic.Apm.Mongo.IntegrationTests
{
    public class MongoApmTests : IClassFixture<MongoFixture<MongoApmTests.MongoConfiguration, BsonDocument>>,
        IDisposable
    {
        public MongoApmTests(MongoFixture<MongoConfiguration, BsonDocument> fixture)
        {
            _documents = fixture.Collection;
            _payloadSender = new MockPayloadSender();

            var config =
                new AgentComponents(configurationReader: new ConfigurationReader(new Uri("http://localhost:8200")),
                    payloadSender: _payloadSender);

            var apmAgentType = typeof(IApmAgent).Assembly.GetType("Elastic.Apm.ApmAgent");
            _agent = (IApmAgent) apmAgentType.GetConstructors(BindingFlags.NonPublic | BindingFlags.Instance).First()
                .Invoke(new object[] {config});
            _agent.Subscribe(new MongoDiagnosticsSubscriber());
        }

        public void Dispose() => (_agent as IDisposable)?.Dispose();

        private readonly IApmAgent _agent;

        private const string DatabaseName = "elastic-apm-mongo";

        public class MongoConfiguration : IMongoConfiguration<BsonDocument>
        {
            public MongoClient GetMongoClient(string connectionString)
            {
                var mongoClientSettings = MongoClientSettings.FromUrl(new MongoUrl(connectionString));
                mongoClientSettings.ClusterConfigurator = builder => builder.Subscribe(new MongoEventSubscriber());

                return new MongoClient(mongoClientSettings);
            }

            string IMongoConfiguration<BsonDocument>.DatabaseName => DatabaseName;
            public string CollectionName => "documents";

            public Task InitializeAsync(IMongoCollection<BsonDocument> collection) =>
                collection.InsertManyAsync(Enumerable.Range(0, 10_000).Select(x => new BsonDocument("_id", x)));

            public Task DisposeAsync(IMongoCollection<BsonDocument> collection) =>
                collection.Database.DropCollectionAsync(CollectionName);
        }

        private readonly IMongoCollection<BsonDocument> _documents;

        private readonly MockPayloadSender _payloadSender;

        [Fact]
        public async Task ApmAgent_ShouldCorrectlyCaptureSpan()
        {
            // Arrange
            var transaction = _agent.Tracer.StartTransaction("elastic-apm-mongo", ApiConstants.TypeDb);

            // Act
            var docs = await _documents
                .Find(Builders<BsonDocument>.Filter.Empty)
                .Project(Builders<BsonDocument>.Projection.ElemMatch("filter", FilterDefinition<BsonDocument>.Empty))
                .ToListAsync();

            transaction.End();

            // Assert
            Assert.Single(_payloadSender.TransactionsQueue);
            Assert.True(_payloadSender.TransactionsQueue.TryPeek(out var capturedTransaction));

            Assert.All(_payloadSender.SpansQueue, span =>
            {
                Assert.Equal(capturedTransaction.Id, span.TransactionId);
                Assert.Equal(DatabaseName, span.Context.Db.Instance);
                Assert.Equal(ApiConstants.TypeDb, span.Type);
            });

            Assert.All(_payloadSender.SpansQueue, span => { Assert.Equal(capturedTransaction.Id, span.ParentId); });
        }

        [Fact]
        public async Task ApmAgent_ShouldCorrectlyCaptureSpanAndError_WhenMongoCommandFailed()
        {
            // Arrange
            var transaction = _agent.Tracer.StartTransaction("elastic-apm-mongo", ApiConstants.TypeDb);

            // Act
            try
            {
                // run failPoint command on non-admin database which is forbidden
                await _documents.Database.RunCommandAsync(new JsonCommand<BsonDocument>(
                    "{configureFailPoint: \"failCommand\", mode: \"alwaysOn\",data: {errorCode: 2, failCommands: [\"find\"]}}"));
                //await _documents.Database.RunCommandAsync(new JsonCommand<BsonDocument>("{}"));
            }
            catch
            {
                // ignore
            }

            transaction.End();

            // Assert
            Assert.Single(_payloadSender.TransactionsQueue);
            Assert.True(_payloadSender.TransactionsQueue.TryPeek(out var capturedTransaction));


            Assert.Single(_payloadSender.SpansQueue);
            Assert.True(_payloadSender.SpansQueue.TryPeek(out var capturedSpan));

            Assert.Equal(capturedTransaction.Id, capturedSpan.TransactionId);
            Assert.Equal(DatabaseName, capturedSpan.Context.Db.Instance);
            Assert.Equal(ApiConstants.TypeDb, capturedSpan.Type);


            Assert.Single(_payloadSender.ErrorsQueue);
            Assert.True(_payloadSender.ErrorsQueue.TryPeek(out var capturedError));

            Assert.Equal(capturedTransaction.Id,
                capturedError.GetType().GetProperty("TransactionId")?.GetValue(capturedError)?.ToString());
            Assert.Equal(capturedSpan.Id,
                capturedError.GetType().GetProperty("ParentId")?.GetValue(capturedError)?.ToString());
        }
    }
}
