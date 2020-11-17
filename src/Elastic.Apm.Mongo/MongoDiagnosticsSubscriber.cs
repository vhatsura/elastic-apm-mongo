using System;
using System.Diagnostics;
using Elastic.Apm.DiagnosticSource;
using Elastic.Apm.Mongo.DiagnosticSource;

// ReSharper disable UnusedMember.Global

namespace Elastic.Apm.Mongo
{
    /// <summary>
    ///     A subscriber to events from mongoDB driver diagnostic source.
    /// </summary>
    public class MongoDiagnosticsSubscriber : IDiagnosticsSubscriber
    {
        /// <summary>
        ///     Starts listening for mongoDB driver diagnostic source events
        /// </summary>
        public IDisposable Subscribe(IApmAgent components)
        {
            var retVal = new CompositeDisposable();

            if (!components.ConfigurationReader.Enabled)
                return retVal;

            var initializer = new MongoDiagnosticInitializer(components);

            retVal.Add(initializer);

            retVal.Add(DiagnosticListener
                .AllListeners
                .Subscribe(initializer));

            return retVal;
        }
    }
}
