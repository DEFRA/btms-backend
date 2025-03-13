using Btms.Backend.IntegrationTests.Helpers;
using Btms.Business.Commands;
using FluentAssertions;
using System.IO.Compression;
using Xunit;
using Xunit.Abstractions;

namespace Btms.Backend.IntegrationTests;

[Trait("Category", "Integration")]
public class DmpIssueDownloadCommandTests(ApplicationFactory factory, ITestOutputHelper testOutputHelper)
    : BaseApiTests(factory, testOutputHelper, "CommandTests", "CDMS-408-DMP-Datalake-Issue"),
        IClassFixture<ApplicationFactory>
{
    [Fact]
    public async Task Download()
    {
        await ClearDb();

        var downloadCommand = new DownloadCommand() { SyncPeriod = SyncPeriod.All };

        var res = await Client.MakeDownloadRequest(downloadCommand);
        var response = await res.Content.ReadAsStringAsync();

        //Remove quotes at start and end
        response = response.Replace("\"", "");

        var zipPath = $"../../../../Btms.Backend/{response}.zip";

        File.Exists(zipPath).Should().BeTrue();

        using ZipArchive archive = ZipFile.OpenRead(zipPath);

        archive.Entries
            .Select(e => e.FullName)
            .Order()
            .Should().Contain(
                new[] {
                    "ALVS/2025/02/02/clearance-request.json",
                    "ALVS/2025/02/02/clearance-request-formatted.json",
                    "DECISIONS/2025/02/02/decision.json",
                    "DECISIONS/2025/02/02/decision-formatted.json",
                    "FINALISATION/2025/02/02/finalisation.json",
                    "FINALISATION/2025/02/02/finalisation-formatted.json"
                }
            );
    }
}