using System;
using System.Collections.Generic;
using Elastic.Apm.Config;
using Elastic.Apm.Helpers;
using Elastic.Apm.Logging;

namespace Elastic.Apm.Mongo.IntegrationTests.Mocks
{
    // todo: use Moq
    public class ConfigurationReader : IConfigurationReader
    {
        public ConfigurationReader(Uri apmServerUrl) => ServerUrls = new[] {apmServerUrl};

        public string CaptureBody => "off";
        public List<string> CaptureBodyContentTypes => new List<string>();
        public bool CaptureHeaders => false;
        public bool CentralConfig => false;
        public IReadOnlyList<WildcardMatcher> DisableMetrics => new List<WildcardMatcher>();
        public string Environment => "test";
        public TimeSpan FlushInterval => TimeSpan.FromSeconds(5);
        public IReadOnlyDictionary<string, string> GlobalLabels => new Dictionary<string, string>();
        public LogLevel LogLevel => LogLevel.Error;
        public int MaxBatchEventCount => 15;
        public int MaxQueueEventCount => 150;
        public double MetricsIntervalInMilliseconds => TimeSpan.FromSeconds(5).TotalMilliseconds;
        public IReadOnlyList<WildcardMatcher> SanitizeFieldNames => new List<WildcardMatcher>();
        public string SecretToken => null;
        public IReadOnlyList<Uri> ServerUrls { get; }
        public string ServiceName => "Elastic.Apm.MongoTests";
        public string ServiceNodeName => "node";
        public string ServiceVersion => "1.0.0";
        public double SpanFramesMinDurationInMilliseconds => 0;
        public int StackTraceLimit => 0;
        public int TransactionMaxSpans => 50;
        public double TransactionSampleRate => 1.0;
        public bool UseElasticTraceparentHeader => false;
        public bool VerifyServerCert => true;
    }
}
