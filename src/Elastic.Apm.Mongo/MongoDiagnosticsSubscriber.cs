using System;
using System.Diagnostics;
using Elastic.Apm.DiagnosticSource;
using Elastic.Apm.Mongo.DiagnosticSource;

namespace Elastic.Apm.Mongo
{
    /// <summary>
    /// </summary>
    public class MongoDiagnosticsSubscriber : IDiagnosticsSubscriber
    {
        /// <summary>
        ///     Starts listening for MongoDB Driver diagnostic source events
        /// </summary>
        /// <param name="components"></param>
        /// <returns></returns>
        public IDisposable Subscribe(IApmAgent components)
        {
            var retVal = new CompositeDisposable();
            var initializer = new MongoDiagnosticInitializer(components);

            retVal.Add(initializer);

            retVal.Add(DiagnosticListener
                .AllListeners
                .Subscribe(initializer));

            return retVal;
        }
    }
}
