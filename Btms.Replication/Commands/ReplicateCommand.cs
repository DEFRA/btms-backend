using System.IO.Compression;
using System.Text.Json.Serialization;
using Btms.BlobService;
using Btms.Business;
using Btms.Business.Commands;
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
        IOptions<BusinessOptions> businessOptions, IOptions<ReplicationOptions> replicationOptions) : IRequestHandler<ReplicateCommand>
    {
        public async Task Handle(ReplicateCommand request, CancellationToken cancellationToken)
        {
            var blobContainer = businessOptions.Value.DmpBlobRootFolder;
            var targetBlobContainer = replicationOptions.Value.DmpBlobContainer;

            if (replicationOptions.Value.Enabled)
            {
                logger.LogInformation("Replicating from {BlobContainer} to {DestinationContainer}", blobContainer, targetBlobContainer);

                DownloadCommand.BlobFolders.ForEach(f =>
                {
                    logger.LogInformation("Replicate {Path}", f.path);
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