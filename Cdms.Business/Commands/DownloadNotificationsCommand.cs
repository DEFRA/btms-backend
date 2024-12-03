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

namespace Cdms.Business.Commands;

public class DownloadCommand : IRequest<DownloadCommand.Result>
{
    [System.Text.Json.Serialization.JsonConverter(typeof(JsonStringEnumConverter<SyncPeriod>))]
    public SyncPeriod SyncPeriod { get; set; }

    public string Path { get; set; } = null!;

    public Type Type { get; set; } = null!;

    internal class Handler(IBlobService blobService, ISensitiveDataSerializer sensitiveDataSerializer, IWebHostEnvironment env) : IRequestHandler<DownloadCommand, DownloadCommand.Result>
    {

        public async Task<Result> Handle(DownloadCommand request, CancellationToken cancellationToken)
        {
            string subFolder = $"{request.Type.Name}\\{Guid.NewGuid()}";
            string rootFolder = System.IO.Path.Combine(env.ContentRootPath, subFolder);
            Directory.CreateDirectory(rootFolder);
            ParallelOptions options = new() { CancellationToken = cancellationToken, MaxDegreeOfParallelism = 10 };
            var result = blobService.GetResourcesAsync($"{request.Path}{request.SyncPeriod.GetPeriodPath()}", cancellationToken);

            //Write local files
            await Parallel.ForEachAsync(result, options, async (item, token) =>
            {
                var blobContent = await blobService.GetResource(item, cancellationToken);
                string redactedContent = sensitiveDataSerializer.RedactRawJson(blobContent, request.Type);
                var filename = System.IO.Path.Combine(rootFolder, item.Name.Replace('/', System.IO.Path.DirectorySeparatorChar));
                Directory.CreateDirectory(System.IO.Path.GetDirectoryName(filename)!);
                await File.WriteAllTextAsync(filename, redactedContent, cancellationToken);
            });

            MemoryStream zipStream = new MemoryStream();
            ZipFile.CreateFromDirectory(rootFolder, zipStream);
            zipStream.Position = 0;
            var commandResult = new Result(zipStream.ToArray());
            Directory.Delete(rootFolder, true);
            return commandResult;
        }

    }

    public record Result(byte[] Zip);
}