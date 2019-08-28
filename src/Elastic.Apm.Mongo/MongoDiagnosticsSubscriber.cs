using System;
using Elastic.Apm.DiagnosticSource;
using MongoDB.Driver.Core.Events;

namespace Elastic.Apm.Mongo
{
    public class MongoDiagnosticsSubscriber : IEventSubscriber, IDiagnosticsSubscriber
    {
        public static MongoDiagnosticsSubscriber Instance => _instance.Value;

        private static readonly Lazy<MongoDiagnosticsSubscriber> _instance =
            new Lazy<MongoDiagnosticsSubscriber>(() => new MongoDiagnosticsSubscriber());

        private MongoDiagnosticsSubscriber()
        {
        }

        private ReflectionEventSubscriber _subscriber;

        public bool TryGetEventHandler<TEvent>(out Action<TEvent> handler)
        {
            handler = null;

            return _subscriber != null && _subscriber.TryGetEventHandler(out handler);
        }

        public IDisposable Subscribe(IApmAgent components)
        {
            var listener = new MongoListener(components);

            _subscriber = new ReflectionEventSubscriber(listener);

            return listener;
        }
    }
}
