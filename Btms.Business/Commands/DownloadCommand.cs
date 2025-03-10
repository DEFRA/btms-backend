using System.IO.Compression;
using System.Text.Json.Serialization;
using Btms.BlobService;
using Btms.Common.Extensions;
using MediatR;
using Btms.SensitiveData;
using Btms.Types.Ipaffs;
using Btms.SyncJob;
using Btms.Types.Alvs;
using Btms.Types.Gvms;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Decision = Btms.Types.Alvs.Decision;

namespace Btms.Business.Commands;

public class DownloadCommand : IRequest, ISyncJob
{
    [JsonConverter(typeof(JsonStringEnumConverter<SyncPeriod>))]
    public SyncPeriod SyncPeriod { get; set; }

    public Guid JobId { get; } = Guid.NewGuid();
    public string Timespan { get; } = null!;
    public string Resource { get; } = null!;

    public DownloadFilter? Filter { get; set; } = null;

    public string? RootFolder { get; set; }

    public static readonly List<(string path, Type dataType)> BlobFolders =
    [
        ("IPAFFS/CHEDA", typeof(ImportNotification)),
        ("IPAFFS/CHEDD", typeof(ImportNotification)),
        ("IPAFFS/CHEDP", typeof(ImportNotification)),
        ("IPAFFS/CHEDPP", typeof(ImportNotification)),
        ("IPAFFS/ALVS", typeof(AlvsClearanceRequest)),
        ("GVMSAPIRESPONSE", typeof(SearchGmrsForDeclarationIdsResponse)),
        ("DECISIONS", typeof(Decision)),
        ("FINALISATION", typeof(Finalisation))
    ];

    internal class Handler(IBlobService blobService, ISensitiveDataSerializer sensitiveDataSerializer, IHostEnvironment env) : IRequestHandler<DownloadCommand>
    {

        public async Task Handle(DownloadCommand request, CancellationToken cancellationToken)
        {
            var rootFolder = Path.Combine(env.ContentRootPath, "temp", request.JobId.ToString());
            Directory.CreateDirectory(rootFolder);

            var datasets = string.IsNullOrEmpty(request.RootFolder) ? DateTimeExtensions.RedactedDatasetsSinceNov24() : [request.RootFolder];

            await datasets.ForEachAsync(async blobContainer =>
            {
                if (request.Filter is not null)
                {
                    await Download(request, rootFolder, $"{blobContainer}/ALVS", typeof(AlvsClearanceRequest), request.Filter.Mrns, cancellationToken);

                    await Download(request, rootFolder, $"{blobContainer}/DECISIONS", typeof(AlvsClearanceRequest), request.Filter.Mrns, cancellationToken);

                    await Download(request, rootFolder, $"{blobContainer}/FINALISATION", typeof(Finalisation), request.Filter.Mrns, cancellationToken);

                    var chedGrouping = request.Filter.Cheds.GroupBy(x => x.Type);
                    foreach (var chedGroup in chedGrouping)
                    {
                        await Download(request, rootFolder, $"{blobContainer}/IPAFFS/{chedGroup.Key}", typeof(ImportNotification), chedGroup.Select(x => x.Identifier).ToArray(), cancellationToken);
                    }
                }
                else
                {
                    await Download(request, rootFolder, $"{blobContainer}/IPAFFS/CHEDA", typeof(ImportNotification), null, cancellationToken);
                    await Download(request, rootFolder, $"{blobContainer}/IPAFFS/CHEDD", typeof(ImportNotification), null, cancellationToken);
                    await Download(request, rootFolder, $"{blobContainer}/IPAFFS/CHEDP", typeof(ImportNotification), null, cancellationToken);
                    await Download(request, rootFolder, $"{blobContainer}/IPAFFS/CHEDPP", typeof(ImportNotification), null, cancellationToken);

                    await Download(request, rootFolder, $"{blobContainer}/ALVS", typeof(AlvsClearanceRequest), null, cancellationToken);

                    await Download(request, rootFolder, $"{blobContainer}/GVMSAPIRESPONSE", typeof(SearchGmrsForDeclarationIdsResponse), null, cancellationToken);

                    await Download(request, rootFolder, $"{blobContainer}/DECISIONS", typeof(AlvsClearanceRequest), null, cancellationToken);

                    await Download(request, rootFolder, $"{blobContainer}/FINALISATION", typeof(Finalisation), null, cancellationToken);
                }
            });



            if (Directory.EnumerateFiles(rootFolder, "*.json", SearchOption.AllDirectories).Any())
            {
                ZipFile.CreateFromDirectory(rootFolder, $"{env.ContentRootPath}/{request.JobId}.zip");
            }

            Directory.Delete(rootFolder, true);
        }

        private async Task Download(DownloadCommand request, string rootFolder, string folder, Type type, string[]? filenameFilter, CancellationToken cancellationToken)
        {
            ParallelOptions options = new() { CancellationToken = cancellationToken, MaxDegreeOfParallelism = 10 };

            var paths = request.SyncPeriod.GetPeriodPaths();

            var tasks = paths
                .Select((periodPath) =>
                    blobService.GetResourcesAsync($"{folder}{periodPath}", cancellationToken)
                )
                .FlattenAsyncEnumerable();

            //Write local files
            await Parallel.ForEachAsync(tasks, options, async (item, _) =>
            {
                bool shouldDownload = true;
                if (filenameFilter is not null)
                {
                    shouldDownload = (from filter in filenameFilter where item.Name.Contains(filter) select item.Name).Any();
                }

                if (shouldDownload)
                {
                    var blobContent = await blobService.GetResource(item, cancellationToken);
                    var redactedContent = sensitiveDataSerializer.RedactRawJson(blobContent, type);
                    var filename = Path.Combine(rootFolder, item.Name.Substring(item.Name.IndexOf('/') + 1).Replace('/', Path.DirectorySeparatorChar));
                    Directory.CreateDirectory(Path.GetDirectoryName(filename)!);
                    await File.WriteAllTextAsync(filename, redactedContent, cancellationToken);
                }
            });
        }

    }

    public record Result(byte[] Zip);

    public record DownloadFilter(string[] Mrns, Ched[] Cheds);

    public record Ched(string Type, string Identifier);


}