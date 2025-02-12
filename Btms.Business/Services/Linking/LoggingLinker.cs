using Microsoft.Extensions.Logging;

namespace Btms.Business.Services.Linking;

/// <summary>
/// Example of linking decoration.
/// </summary>
/// <param name="inner"></param>
/// <param name="logger"></param>
/// <typeparam name="TModel"></typeparam>
/// <typeparam name="TKModel"></typeparam>
public class LoggingLinker<TModel, TKModel>(
    ILinker<TModel, TKModel> inner,
    ILogger<LoggingLinker<TModel, TKModel>> logger) : ILinker<TModel, TKModel>
    where TModel : class
    where TKModel : class
{
    public async Task Link(TKModel model, CancellationToken cancellationToken)
    {
        var timestamp = TimeProvider.System.GetTimestamp();

        try
        {
            await inner.Link(model, cancellationToken);
        }
        finally
        {
            var elapsed = TimeProvider.System.GetElapsedTime(timestamp);
            logger.LogInformation("Linking {TModel} with {TKModel} took {Elapsed}ms", typeof(TModel).Name,
                typeof(TKModel), elapsed);
        }
    }
}