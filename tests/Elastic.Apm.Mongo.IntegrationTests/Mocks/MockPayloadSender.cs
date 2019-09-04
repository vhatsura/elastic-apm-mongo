using System.Collections.Concurrent;
using Elastic.Apm.Api;
using Elastic.Apm.Report;

namespace Elastic.Apm.Mongo.IntegrationTests.Mocks
{
    public class MockPayloadSender : IPayloadSender
    {
        public ConcurrentQueue<IError> ErrorsQueue { get; } = new ConcurrentQueue<IError>();
        public ConcurrentQueue<ITransaction> TransactionsQueue { get; } = new ConcurrentQueue<ITransaction>();
        public ConcurrentQueue<ISpan> SpansQueue { get; } = new ConcurrentQueue<ISpan>();

        public void QueueError(IError error) => ErrorsQueue.Enqueue(error);

        public void QueueTransaction(ITransaction transaction) => TransactionsQueue.Enqueue(transaction);

        public void QueueSpan(ISpan span) => SpansQueue.Enqueue(span);

        public void QueueMetrics(IMetricSet metrics)
        {
        }

        public void DropQueues()
        {
            ErrorsQueue.Clear();
            TransactionsQueue.Clear();
            SpansQueue.Clear();
        }
    }
}
