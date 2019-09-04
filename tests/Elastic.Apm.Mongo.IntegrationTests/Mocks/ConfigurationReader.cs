using System;
using System.Collections.Generic;
using Elastic.Apm.Config;
using Elastic.Apm.Logging;

namespace Elastic.Apm.Mongo.IntegrationTests.Mocks
{
    public class ConfigurationReader : IConfigurationReader
    {
        public ConfigurationReader(Uri apmServerUrl) => ServerUrls = new[] {apmServerUrl};

        public bool CaptureHeaders => false;
        public LogLevel LogLevel => LogLevel.Error;
        public double MetricsIntervalInMilliseconds => TimeSpan.FromSeconds(5).TotalMilliseconds;
        public string SecretToken => null;
        public IReadOnlyList<Uri> ServerUrls { get; }
        public string ServiceName => "Elastic.Apm.MongoTests";
        public double SpanFramesMinDurationInMilliseconds => 0;
        public int StackTraceLimit => 0;
        public double TransactionSampleRate => 1.0;
    }
}
