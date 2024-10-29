using Cdms.BlobService;
using Cdms.SensitiveData;
using Microsoft.Extensions.Logging;
using SlimMessageBus;
using System.Text.Json.Serialization;
using IRequest = MediatR.IRequest;

namespace Cdms.Business.Commands;

public enum SyncPeriod
{
    Today,
    LastMonth,
    ThisMonth,
    All
}

public class SyncCommand : IRequest
{
    [JsonConverter(typeof(JsonStringEnumConverter<SyncPeriod>))]
    public SyncPeriod SyncPeriod { get; set; }

    internal abstract class Handler<T>(
        IPublishBus bus,
        ILogger<T> logger,
        ISensitiveDataSerializer sensitiveDataSerializer,
        IBlobService blobService)
        : MediatR.IRequestHandler<T>
        where T : IRequest
    {
        public abstract Task Handle(T request, CancellationToken cancellationToken);

        protected async Task<Status> SyncBlobPaths<T>(SyncPeriod period, string topic, params string[] paths)
        {
            logger.LogInformation($"SyncNotifications period={period}");
            try
            {
                var itemCount = 0;
                var erroredCount = 0;

                // TODO need to figure out how we select path

                await Parallel.ForEachAsync(paths, async (path, token) =>
                {
                    var (e, i) = await SyncBlobPath<T>($"{path}{GetPeriodPath(period)}", topic);
                    itemCount += i;
                    erroredCount += e;
                });


                return new Status()
                {
                    Success = true, Description = $"Connected. {itemCount} items upserted. {erroredCount} errors."
                };
            }
            catch (Exception ex)
            {
                logger.LogError(ex.ToString());

                return new Status() { Success = false, Description = ex.Message };
            }
        }

        protected async Task<(int, int)> SyncBlobPath<T>(string path, string topic)
        {
            var itemCount = 0;
            var erroredCount = 0;

            try
            {
                // TODO need to figure out how we select path

                var result = blobService.GetResourcesAsync(path);

                await foreach (IBlobItem item in result) //) //
                {
                    try
                    {
                        var blobContent = await item.Download();
                        var message = sensitiveDataSerializer.Deserialize<T>(blobContent, _ => { })!;
                        await bus.Publish(message,
                            topic,
                            headers: new Dictionary<string, object>() { { "messageId", item.Name } });
                        itemCount++;
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(
                            $"Failed to upsert ipaffs notification from file {item.Name}. {ex.ToString()}.");

                        erroredCount++;
                    }
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex.ToString());
            }

            return (erroredCount, itemCount);
        }

        private static string GetPeriodPath(SyncPeriod period)
        {
            if (period == SyncPeriod.LastMonth)
            {
                return DateTime.Today.AddMonths(-1).ToString("/yyyy/MM/");
            }
            else if (period == SyncPeriod.ThisMonth)
            {
                return DateTime.Today.ToString("/yyyy/MM/");
            }
            else if (period == SyncPeriod.LastMonth)
            {
                return DateTime.Today.AddMonths(-1).ToString("/yyyy/MM/");
            }
            else if (period == SyncPeriod.Today)
            {
                return DateTime.Today.ToString("/yyyy/MM/dd/");
            }
            else if (period == SyncPeriod.All)
            {
                return "/";
            }
            else
            {
                throw new Exception($"Unexpected SyncPeriod {period}");
            }
        }
    }
}