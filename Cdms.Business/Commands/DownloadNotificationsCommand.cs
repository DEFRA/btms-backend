using System.Dynamic;
using System.IO;
using System.IO.Compression;
using System.Text.Json.Serialization;
using Bogus;
using Cdms.BlobService;
using System.Threading;
using MediatR;
using Newtonsoft.Json;
using JsonSerializer = System.Text.Json.JsonSerializer;
using SharpCompress.Writers;
using Cdms.SensitiveData;
using Cdms.Types.Ipaffs;
using Microsoft.AspNetCore.Hosting;
using Cdms.SyncJob;
using Cdms.Types.Alvs;
using Cdms.Types.Gvms;

namespace Cdms.Business.Commands;

public class DownloadCommand : IRequest, ISyncJob
{
    [System.Text.Json.Serialization.JsonConverter(typeof(JsonStringEnumConverter<SyncPeriod>))]
    public SyncPeriod SyncPeriod { get; set; }

    public Guid JobId { get; } = Guid.NewGuid();
    public string Timespan { get; } = null!;
    public string Resource { get; } = null!;

    internal class Handler(IBlobService blobService, ISensitiveDataSerializer sensitiveDataSerializer, IWebHostEnvironment env) : IRequestHandler<DownloadCommand>
    {

        public async Task Handle(DownloadCommand request, CancellationToken cancellationToken)
        {
            string subFolder = $"temp\\{request.JobId}";
            string rootFolder = Path.Combine(env.ContentRootPath, subFolder);
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
            await Parallel.ForEachAsync(result, options, async (item, token) =>
            {
                var blobContent = await blobService.GetResource(item, cancellationToken);
                string redactedContent = sensitiveDataSerializer.RedactRawJson(blobContent, type);
                var filename = System.IO.Path.Combine(rootFolder, item.Name.Replace('/', System.IO.Path.DirectorySeparatorChar));
                Directory.CreateDirectory(System.IO.Path.GetDirectoryName(filename)!);
                await File.WriteAllTextAsync(filename, redactedContent, cancellationToken);
            });
        }

    }

    public record Result(byte[] Zip);

   
}