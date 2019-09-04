using System;
using System.Diagnostics;

namespace Elastic.Apm.Mongo.DiagnosticSource
{
    internal sealed class MongoDiagnosticInitializer : IObserver<DiagnosticListener>, IDisposable
    {
        private readonly IApmAgent _apmAgent;

        private IDisposable _sourceSubscription;

        internal MongoDiagnosticInitializer(IApmAgent apmAgent) => _apmAgent = apmAgent;

        public void Dispose() => _sourceSubscription?.Dispose();

        public void OnCompleted()
        {
        }

        public void OnError(Exception error)
        {
        }

        public void OnNext(DiagnosticListener value)
        {
            if (value.Name == Constants.MongoDiagnosticName)
                _sourceSubscription = value.Subscribe(new MongoDiagnosticListener(_apmAgent));
        }
    }
}
