using System.IO.Compression;
using System.Text.Json.Serialization;
using Btms.BlobService;
using MediatR;
using Btms.SensitiveData;
using Btms.Types.Ipaffs;
using Btms.SyncJob;
using Btms.Types.Alvs;
using Btms.Types.Gvms;
using Microsoft.Extensions.Hosting;

namespace Btms.Business.Commands;

public class DownloadCommand : IRequest, ISyncJob
{
    [JsonConverter(typeof(JsonStringEnumConverter<SyncPeriod>))]
    public SyncPeriod SyncPeriod { get; set; }

    public Guid JobId { get; } = Guid.NewGuid();
    public string Timespan { get; } = null!;
    public string Resource { get; } = null!;

    internal class Handler(IBlobService blobService, ISensitiveDataSerializer sensitiveDataSerializer, IHostEnvironment env) : IRequestHandler<DownloadCommand>
    {

        public async Task Handle(DownloadCommand request, CancellationToken cancellationToken)
        {
            var subFolder = $"temp\\{request.JobId}";
            var rootFolder = Path.Combine(env.ContentRootPath, subFolder);
            Directory.CreateDirectory(rootFolder);

            await Download(request, rootFolder, "RAW/IPAFFS/CHEDA", typeof(ImportNotification), cancellationToken);
            await Download(request, rootFolder, "RAW/IPAFFS/CHEDD", typeof(ImportNotification), cancellationToken);
            await Download(request, rootFolder, "RAW/IPAFFS/CHEDP", typeof(ImportNotification), cancellationToken);
            await Download(request, rootFolder, "RAW/IPAFFS/CHEDPP", typeof(ImportNotification), cancellationToken);

            await Download(request, rootFolder, "RAW/ALVS", typeof(AlvsClearanceRequest), cancellationToken);

            await Download(request, rootFolder, "RAW/GVMSAPIRESPONSE", typeof(SearchGmrsForDeclarationIdsResponse), cancellationToken);

            await Download(request, rootFolder, "RAW/DECISIONS", typeof(AlvsClearanceRequest), cancellationToken);
            
            ZipFile.CreateFromDirectory(rootFolder, $"{env.ContentRootPath}\\{request.JobId}.zip");
           
            Directory.Delete(rootFolder, true);
        }

        private async Task Download(DownloadCommand request, string rootFolder, string folder, Type type, CancellationToken cancellationToken)
        {
            
            ParallelOptions options = new() { CancellationToken = cancellationToken, MaxDegreeOfParallelism = 10 };
            var result = blobService.GetResourcesAsync($"{folder}{request.SyncPeriod.GetPeriodPath()}", cancellationToken);

            //Write local files
            await Parallel.ForEachAsync(result, options, async (item, _) =>
            {
                var blobContent = await blobService.GetResource(item, cancellationToken);
                var redactedContent = sensitiveDataSerializer.RedactRawJson(blobContent, type);
                var filename = Path.Combine(rootFolder, item.Name.Replace('/', Path.DirectorySeparatorChar));
                Directory.CreateDirectory(Path.GetDirectoryName(filename)!);
                await File.WriteAllTextAsync(filename, redactedContent, cancellationToken);
            });
        }

    }

    public record Result(byte[] Zip);

   
}