using System.IO.Compression;
using Btms.Business.Commands;
using Btms.Model;
using Btms.Model.Ipaffs;
using CommandLine;
using MediatR;
using Refit;

namespace Btms.Backend.Cli.Features.DownloadScenarioData;

[Verb("download-scenario-data", isDefault: false, HelpText = "Generates Csharp ALVS classes from XSD Schema.")]
internal class DownloadScenarioDataCommand : IRequest
{
    [Option('e', "environment", Required = false, HelpText = "The environment to run against.  Local, Dev, Test")]
    public string Environment { get; set; } = "Local";

    [Option('o', "outputFolder", Required = true, HelpText = "The folder where to download the data too")]
    public required string OutputFolder { get; set; }

    [Option('r', "rootFolder", Required = false, HelpText = "The root folder to search within, the default will be 'RAW'")]
    public string RootFolder { get; set; } = "RAW";

    [Option('p', "period", Required = false, HelpText = "The period which to search over.  Current options are [Today, LastMonth, ThisMonth, All].  If not specified, will default to All")]
    public string Period { get; set; } = "All";

    [Option('c', "clearance-request", Required = true, HelpText = "MRN number to filter on")]

    public IEnumerable<string> ClearanceRequests { get; set; } = [];

    

    public class Handler : IRequestHandler<DownloadScenarioDataCommand>
    {
        public async Task Handle(DownloadScenarioDataCommand request, CancellationToken cancellationToken)
        {
            var btmsApi = GetApi(request.Environment);

            var history = await btmsApi.GetMovementTimeline(request.ClearanceRequests.First());
            
            Console.WriteLine(history);

            var mrns = history.Items.Where(x => x.ResourceType == nameof(Movement)).Distinct().Select(x => x.ResourceId)
                .ToArray();

            var cheds = history.Items.Where(x => x.ResourceType == nameof(ImportNotification)).Distinct()
                .Select(x => new DownloadCommand.Ched(x.ResourceId.Split('.')[0], x.ResourceId.Split('.')[^1]))
                .ToArray();

            DownloadCommand apiCommand = new DownloadCommand()
            {
                RootFolder = request.RootFolder,
                SyncPeriod = Enum.Parse<SyncPeriod>(request.Period),
                Filter = new DownloadCommand.DownloadFilter(mrns, cheds)
            };
           

            var apiResult = await btmsApi.GenerateDownload(apiCommand);
            Console.WriteLine(apiResult);

            await btmsApi.WaitOnJobCompleting(apiResult.Trim('"'));

            var downloadResult = await btmsApi.DownloadFile(apiResult.Trim('"'));
            
            ZipFile.ExtractToDirectory(downloadResult, request.OutputFolder);
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