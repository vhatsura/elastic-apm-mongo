using System;
using System.Collections.Concurrent;
using Elastic.Apm.Api;
using Elastic.Apm.Logging;
using MongoDB.Driver.Core.Events;

namespace Elastic.Apm.Mongo
{
    public class MongoListener : IDisposable
    {
        private readonly IApmAgent _apmAgent;
        private readonly IApmLogger _logger;

        private readonly ConcurrentDictionary<int, ISpan> _processingQueries = new ConcurrentDictionary<int, ISpan>();

        public MongoListener(IApmAgent apmAgent)
        {
            _apmAgent = apmAgent;
            _logger = _apmAgent.Logger;
            //_apmAgent.Logger?.Scoped();
            // LoggingExtensions is internal

            // {"@timestamp":"2019-08-26T22:06:29.6053995+03:00","level":"Information",
            // "messageTemplate":"{{{Scope}}} The agent was started without a service name. The automatically discovered service name is {ServiceName}",
            // "message":"{{Scope}} The agent was started without a service name. The automatically discovered service name is \"ServiceName\"",
            // "{Scope}":"MicrosoftExtensionsConfig","ServiceName":"ServiceName","SourceContext":"Elastic.Apm"}
        }

        public void Handle(CommandStartedEvent @event)
        {
            var transaction = _apmAgent.Tracer.CurrentTransaction;
            if (transaction == null)
            {
                return;
            }

            var currentExecutionSegment = _apmAgent.Tracer.CurrentSpan ?? (IExecutionSegment) transaction;
            var span = currentExecutionSegment.StartSpan(
                @event.CommandName,
                ApiConstants.TypeDb,
                "mongo");

            _processingQueries.TryAdd(@event.RequestId, span);
            
            span.Action = ApiConstants.ActionQuery;

            span.Context.Db = new Database
            {
                Statement = @event.Command.ToString(),
                Instance = @event.ConnectionId.ServerId.EndPoint.ToString(),
                Type = "mongo"
            };
        }

        public void Handle(CommandSucceededEvent @event)
        {
            if (_processingQueries.TryRemove(@event.RequestId, out var span))
            {
                span.Duration = @event.Duration.TotalMilliseconds;
            }

            span.End();
        }

        public void Handle(CommandFailedEvent @event)
        {
            if (_processingQueries.TryRemove(@event.RequestId, out var span))
            {
                span.Duration = @event.Duration.TotalMilliseconds;
            }

            span.CaptureException(@event.Failure);
        }

        public void Dispose()
        {
        }
    }
}
