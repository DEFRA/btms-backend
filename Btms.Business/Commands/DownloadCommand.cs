using System.IO.Compression;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using Btms.BlobService;
using Btms.Business.Extensions;
using Btms.Common.Extensions;
using Btms.SensitiveData;
using Btms.SyncJob;
using Btms.Types.Alvs;
using Btms.Types.Gvms;
using Btms.Types.Ipaffs;
using Json.Path;
using MediatR;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Decision = Btms.Types.Alvs.Decision;

namespace Btms.Business.Commands;

public class DownloadCommand : IRequest, ISyncJob
{
    [JsonConverter(typeof(JsonStringEnumConverter<SyncPeriod>))]
    public SyncPeriod SyncPeriod { get; init; }

    public Guid JobId { get; } = Guid.NewGuid();
    public string Timespan => SyncPeriod.ToString();
    public string Resource { get; } = "Download";

    public DownloadFilter? Filter { get; set; } = null;

    public string? RootFolder { get; set; }

    public static readonly List<(string path, Type dataType)> BlobFolders =
    [
        ("IPAFFS/CHEDA", typeof(ImportNotification)),
        ("IPAFFS/CHEDD", typeof(ImportNotification)),
        ("IPAFFS/CHEDP", typeof(ImportNotification)),
        ("IPAFFS/CHEDPP", typeof(ImportNotification)),
        ("ALVS", typeof(AlvsClearanceRequest)),
        ("GVMSAPIRESPONSE", typeof(SearchGmrsForDeclarationIdsResponse)),
        ("DECISIONS", typeof(Decision)),
        ("FINALISATION", typeof(Finalisation))
    ];

    internal class Handler(ILogger<DownloadCommand> logger, IOptions<BusinessOptions> businessOptions, IBlobService blobService, ISensitiveDataSerializer sensitiveDataSerializer, IHostEnvironment env, ISyncJobStore syncJobStore) : IRequestHandler<DownloadCommand>
    {

        private List<string> GetDataSets(DownloadCommand request)
        {
            List<string> datasets = [businessOptions.Value.DmpBlobRootFolder];

            if (!string.IsNullOrEmpty(request.RootFolder))
            {
                datasets = [request.RootFolder];
            }
            else if (request.Filter is not null)
            {
                datasets = DateTimeExtensions.RedactedDatasetsSinceNov24();
            }

            return datasets;
        }

        public async Task Handle(DownloadCommand request, CancellationToken cancellationToken)
        {
            var rootFolder = Path.Combine(businessOptions.Value.DownloadFolder ?? env.ContentRootPath, "temp", request.JobId.ToString());
            var job = syncJobStore.GetJob(request.JobId)!;
            job.Start();

            Directory.CreateDirectory(rootFolder);

            var datasets = GetDataSets(request);

            var references = (request.Filter?.Mrns ?? [])
                .Concat(request.Filter?.Cheds.Select(c => c.Reference).ToArray() ?? [])
                .ToArray();

            await datasets.ForEachAsync(async blobContainer =>
            {
                if (request.Filter is not null)
                {

                    await BlobFolders.ForEachAsync(async f =>
                    {
                        logger.LogInformation("Searching dataset {Dataset} Folder {Path}, {DataType} for references {References}", blobContainer, f.path, f.dataType, references);

                        if (f.dataType != typeof(ImportNotification))
                        {
                            await Download(request, rootFolder, $"{blobContainer}/{f.path}", f.dataType, request.Filter.Mrns,
                                job, cancellationToken);
                        }
                        else
                        {
                            //There are multiple paths to search for cheds, so need to search them all based on the cheds we're trying to find
                            var chedGrouping = request.Filter.Cheds.GroupBy(x => x.Type);

                            foreach (var chedGroup in chedGrouping)
                            {
                                await Download(request, rootFolder, $"{blobContainer}/IPAFFS/{chedGroup.Key}", typeof(ImportNotification), chedGroup.Select(x => x.Identifier).ToArray(), job, cancellationToken);
                            }
                        }
                    });
                }
                else
                {
                    await BlobFolders.ForEachAsync(async f =>
                    {
                        logger.LogInformation("Downloading entire dataset {Dataset} Folder {Path}, {DataType}", blobContainer, f.path, f.dataType);

                        await Download(request, rootFolder, $"{blobContainer}/{f.path}", f.dataType, null,
                            job, cancellationToken);
                    });
                }
            });

            if (Directory.EnumerateFiles(rootFolder, "*.json", SearchOption.AllDirectories).Any())
            {
                logger.LogInformation("{Count} files found for download References={Mrns} in {Path}", job.MessagesProcessed, references, rootFolder);
                var zipFilePath = $"{env.ContentRootPath}/{request.JobId}.zip";
                ZipFile.CreateFromDirectory(rootFolder, zipFilePath);
                logger.LogInformation("Zipfile created in {Path}", zipFilePath);
            }
            else
            {
                logger.LogWarning("No files found for download References={Mrns} in {Path}", references, rootFolder);
            }

            logger.LogInformation("Deleting directory {Path}", rootFolder);
            Directory.Delete(rootFolder, true);

            job.Complete();
        }

        private async Task Download(DownloadCommand request, string rootFolder, string folder, Type type, string[]? filenameFilter, SyncJob.SyncJob job, CancellationToken cancellationToken)
        {
            ParallelOptions options = new() { CancellationToken = cancellationToken, MaxDegreeOfParallelism = 10 };

            var tasks = blobService.GetBlobItems(folder, request.SyncPeriod, cancellationToken);

            //Write local files
            await Parallel.ForEachAsync(tasks, options, async (item, _) =>
            {
                logger.LogWarning("Processing file {Name}", item.Name);
                job.BlobSuccess();

                try
                {
                    bool shouldDownload = true;
                    if (filenameFilter is not null)
                    {
                        shouldDownload = (from filter in filenameFilter where item.Name.Contains(filter) select item.Name).Any();
                    }

                    if (shouldDownload)
                    {
                        var blobContent = await blobService.GetResource(item, cancellationToken);

                        (type, var fileParts) = item.EnsureCorrectTypeAndFilePath(type, blobContent);

                        var redactedContent = sensitiveDataSerializer.RedactRawJson(blobContent, type);
                        var filename = Path.Combine(rootFolder, String.Join(Path.DirectorySeparatorChar, fileParts));

                        Directory.CreateDirectory(Path.GetDirectoryName(filename)!);
                        await File.WriteAllTextAsync(filename, redactedContent, cancellationToken);

                        job.MessageProcessed();
                    }
                }
                catch
                {
                    job.MessageFailed();
                }
            });
        }


    }

    public record Result(byte[] Zip);

    public record DownloadFilter(string[] Mrns, Ched[] Cheds);

    public record Ched(string Reference, string Type, string Identifier)
    {
        public static Ched FromReference(string reference)
        {
            return new Ched(reference, reference.Split('.')[0], reference.Split('.')[^1]);
        }
    };


}