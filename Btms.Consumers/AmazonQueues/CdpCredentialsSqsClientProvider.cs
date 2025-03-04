using Amazon.SQS;
using SlimMessageBus.Host.AmazonSQS;

namespace Btms.Consumers.AmazonQueues;

public class CdpCredentialsSqsClientProvider(AmazonSQSConfig sqsConfig) : ISqsClientProvider, IDisposable
{
    private bool _disposedValue;

    private readonly AmazonSQSClient _client = new(sqsConfig);

    #region ISqsClientProvider

    public AmazonSQSClient Client => _client;

    public Task EnsureClientAuthenticated() => Task.CompletedTask;

    #endregion

    #region Dispose Pattern

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposedValue)
        {
            if (disposing)
            {
                _client?.Dispose();
            }

            _disposedValue = true;
        }
    }

    public void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    #endregion
}