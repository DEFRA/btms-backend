using System.IO.Compression;
using Btms.Business.Commands;
using Btms.Model;
using Btms.Model.Ipaffs;
using CommandLine;
using MediatR;
using Microsoft.Extensions.Logging;
using Refit;

namespace Btms.Backend.Cli.Features.DownloadScenarioData;

[Verb("download-scenario-data", isDefault: false, HelpText = "Generates Csharp ALVS classes from XSD Schema.")]
internal class DownloadScenarioDataCommand : IRequest
{
    [Option('e', "environment", Required = false, HelpText = "The environment to run against.  Local, Dev, Test")]
    public string Environment { get; set; } = "Local";

    [Option('o', "outputFolder", Required = false, HelpText = "The folder where to download the data too, defaults to the current directory")]
    public string OutputFolder { get; set; } = Directory.GetCurrentDirectory();

    [Option('r', "rootFolder", Required = false, HelpText = "The root folder to search within, this will default to the API default if not set")]
    public string? RootFolder { get; set; }

    [Option('p', "period", Required = false, HelpText = "The period which to search over.  Current options are [Today, LastMonth, ThisMonth, All].  If not specified, will default to All")]
    public string Period { get; set; } = "All";

    [Option('c', "clearance-request", Required = true, HelpText = "MRN number to filter on")]

    public required string ClearanceRequest { get; set; }



    public class Handler(ILogger<DownloadScenarioDataCommand> logger) : IRequestHandler<DownloadScenarioDataCommand>
    {
        public async Task Handle(DownloadScenarioDataCommand request, CancellationToken cancellationToken)
        {
            var btmsApi = GetApi(request.Environment);

            logger.LogInformation("Fetching Movement timeline for MRN: {Mrn} on environment {Environment}", request.ClearanceRequest, request.Environment);

            var history = await btmsApi.GetMovementTimeline(request.ClearanceRequest);

            logger.LogInformation("Timeline for MRN: {Mrn} History items found {HistoryCount}", request.ClearanceRequest, history.Items.Count);

            var cheds = history.Items.Where(x => x.ResourceType == nameof(ImportNotification)).Distinct()
                .Select(x => new DownloadCommand.Ched(x.ResourceId.Split('.')[0], x.ResourceId.Split('.')[^1]))
                .ToArray();

            DownloadCommand apiCommand = new DownloadCommand()
            {
                RootFolder = request.RootFolder,
                SyncPeriod = Enum.Parse<SyncPeriod>(request.Period),
                Filter = new DownloadCommand.DownloadFilter([request.ClearanceRequest], cheds)
            };

            logger.LogInformation("Starting Download Job on API");

            var apiResult = await btmsApi.GenerateDownload(apiCommand);
            string jobId = apiResult.Trim('"');

            logger.LogInformation("Waiting on Download Job to complete: {JobId}", jobId);

            await btmsApi.WaitOnJobCompleting(apiResult.Trim('"'));

            logger.LogInformation("Job Completed, Downloading zip file");

            var downloadResult = await btmsApi.DownloadFile(apiResult.Trim('"'));

            logger.LogInformation("Extracting Download to {OutputFolder}", request.OutputFolder);

            ZipFile.ExtractToDirectory(downloadResult, request.OutputFolder);

            logger.LogInformation("Completed");

        }


        private static IBtmsApi GetApi(string environment)
        {
            return environment switch
            {
                "Local" => RestService.For<IBtmsApi>("http://localhost:5002"),
                "Dev" => RestService.For<IBtmsApi>("https://btms-backend.dev.cdp-int.defra.cloud"),
                "Test" => RestService.For<IBtmsApi>("https://btms-backend.test.cdp-int.defra.cloud"),
                _ => throw new ArgumentException("Invalid Environment", nameof(environment))
            };
        }
    }


}