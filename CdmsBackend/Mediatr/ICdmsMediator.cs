using Cdms.SyncJob;
using MediatR;

namespace CdmsBackend.Mediatr;

public interface ICdmsMediator
{
    Task SendSyncJob<TRequest>(TRequest request, CancellationToken cancellationToken = default)
        where TRequest : IRequest, ISyncJob;

    Task SendJob<TRequest>(TRequest request, CancellationToken cancellationToken = default)
        where TRequest : IRequest, ISyncJob;



    /// <summary>
    /// Asynchronously send a request to a single handler
    /// </summary>
    /// <typeparam name="TResponse">Response type</typeparam>
    /// <param name="request">Request object</param>
    /// <param name="cancellationToken">Optional cancellation token</param>
    /// <returns>A task that represents the send operation. The task result contains the handler response</returns>
    Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Asynchronously send a request to a single handler with no response
    /// </summary>
    /// <param name="request">Request object</param>
    /// <param name="cancellationToken">Optional cancellation token</param>
    /// <returns>A task that represents the send operation.</returns>
    Task Send<TRequest>(TRequest request, CancellationToken cancellationToken = default)
        where TRequest : IRequest;
}