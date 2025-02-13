using System.IO.Compression;
using Amazon.Runtime.Internal.Util;
using Btms.Backend.Cli.Features.DownloadScenarioData;
using Btms.Business.Commands;
using Btms.Model.Ipaffs;
using Btms.SensitiveData;
using CommandLine;
using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Refit;
using ImportNotification = Btms.Types.Ipaffs.ImportNotification;

namespace Btms.Backend.Cli.Features.RedactImportNotifications;

[Verb("redact-import-notifications", isDefault: false, HelpText = "Redacts Import Notification files.")]
internal class RedactImportNotificationsCommand : IRequest
{
    [Option('r', "rootFolder", Required = true, HelpText = "The root folder to search within")]
    public required string RootFolder { get; set; }

    public class Handler(ILogger<RedactImportNotificationsCommand> logger) : IRequestHandler<RedactImportNotificationsCommand>
    {
        public async Task Handle(RedactImportNotificationsCommand request, CancellationToken cancellationToken)
        {
            DirectoryInfo di =
                new DirectoryInfo(request.RootFolder);

            var files = di.GetFiles("*.json", SearchOption.AllDirectories);

            logger.LogInformation("Found {Count} files", files.Length);

            await Parallel.ForEachAsync(files, cancellationToken, async (fileInfo, ct) =>
            {
                logger.LogInformation("Starting file {File}", fileInfo.FullName);
                var json = await File.ReadAllTextAsync(fileInfo.FullName, ct);

                var options = new SensitiveDataOptions { Include = false };
                var serializer =
                    new SensitiveDataSerializer(Options.Create(options), NullLogger<SensitiveDataSerializer>.Instance);

                var result = serializer.RedactRawJson(json, typeof(ImportNotification));
                await File.WriteAllTextAsync(fileInfo.FullName, result, ct);

                logger.LogInformation("Completed file {File}", fileInfo.FullName);
            });

        }
    }


}