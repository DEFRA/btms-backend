using System.IO.Compression;
using System.Text.Json.Serialization;
using Btms.BlobService;
using Btms.Business;
using Btms.Business.Commands;
using Btms.Business.Extensions;
using Btms.Common.Extensions;
using MediatR;
using Btms.SensitiveData;
using Btms.Types.Ipaffs;
using Btms.SyncJob;
using Btms.Types.Alvs;
using Btms.Types.Gvms;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Btms.Replication.Commands;

public class ReplicateCommand() : IRequest, ISyncJob
{
    [JsonConverter(typeof(JsonStringEnumConverter<SyncPeriod>))]
    public SyncPeriod SyncPeriod { get; set; }

    public Guid JobId { get; } = Guid.NewGuid();
    public string Timespan { get; } = null!;
    public string Resource { get; } = null!;

    public string? RootFolder { get; set; }

    internal class Handler(ILogger<ReplicateCommand> logger, IBlobService blobService, ISensitiveDataSerializer sensitiveDataSerializer, ReplicationTargetBlobService replicationTargetBlobService,
        IOptions<BusinessOptions> businessOptions, IOptions<ReplicationOptions> replicationOptions, ISyncJobStore syncJobStore) : IRequestHandler<ReplicateCommand>
    {
        public async Task Handle(ReplicateCommand request, CancellationToken cancellationToken)
        {
            var job = syncJobStore.GetJob(request.JobId)!;
            job.Start();

            var blobFolder = businessOptions.Value.DmpBlobRootFolder;
            var targetBlobContainer = replicationOptions.Value.DmpBlobContainer;
            var targetBlobFolder = replicationOptions.Value.DmpBlobRootFolder;

            if (replicationOptions.Value.Enabled)
            {
                logger.LogInformation("Replicating from {BlobFolder} to {DestinationContainer}/{DestinationFolder}", blobFolder, targetBlobContainer, targetBlobFolder);

                await DownloadCommand.BlobFolders.ForEachAsync(async f =>
                {
                    logger.LogInformation("Replicate {Path}", f.path);

                    var blobs = blobService.GetBlobItems($"{blobFolder}/{f.path}", request.SyncPeriod, cancellationToken);

                    await Parallel.ForEachAsync(blobs, cancellationToken, async (item, ct) =>
                    {
                        logger.LogWarning("Processing file {Name}", item.Name);

                        try
                        {
                            var blobContent = await blobService.GetResource(item, ct);

                            job.BlobSuccess();

                            try
                            {
                                var type = f.dataType;

                                (type, var fileParts) = item.EnsureCorrectTypeAndFilePath(type, blobContent);

                                //Redact
                                var redactedContent = sensitiveDataSerializer.RedactRawJson(blobContent, type);

                                //Write to target
                                var filename = Path.Combine(replicationOptions.Value.DmpBlobRootFolder,
                                    String.Join(Path.DirectorySeparatorChar, fileParts));

                                logger.LogWarning("Writing file to {Name}/{PATH}",
                                    replicationOptions.Value.DmpBlobContainer, filename);

                                await replicationTargetBlobService.WriteResource(filename, redactedContent,
                                    ct);

                                job.MessageProcessed();

                            }
                            catch (Exception ex)
                            {
                                logger.LogWarning("Failed to process and replicated file {Name}. {Error}", item.Name, ex.Message);
                                job.MessageFailed();
                            }
                        }
                        catch (Exception ex)
                        {
                            logger.LogError("Failed to read file {Name}. {Error}", item.Name, ex.Message);
                            job.BlobFailed();
                        }

                    });
                });
            }
            else
            {
                logger.LogWarning("ReplicateCommand called but replication not enabled in config.");
            }

            await Task.CompletedTask;
        }

    }
}