using System.Diagnostics;
using System.Text.Json;
using Btms.Backend.IntegrationTests.Helpers;
using Btms.Business.Commands;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;

namespace Btms.Backend.IntegrationTests;

[Trait("Category", "Integration")]
public class PhaScenarioTests(ApplicationFactory factory, ITestOutputHelper testOutputHelper)
    : BaseApiTests(factory, testOutputHelper), IClassFixture<ApplicationFactory>
{
    private bool _saveData = false;
    
    [Fact]
    public async Task SyncClearanceRequests_WithReferencedNotifications_ShouldLink()
    {
        await ClearDb();
        await Client.MakeSyncNotificationsRequest(new SyncNotificationsCommand
        {
            SyncPeriod = SyncPeriod.All, RootFolder = "PhaScenarios"
        });
        await Client.MakeSyncClearanceRequest(new SyncClearanceRequestsCommand
        {
            SyncPeriod = SyncPeriod.All, RootFolder = "PhaScenarios"
        });
        await Client.MakeSyncDecisionsRequest(new SyncDecisionsCommand
        {
            SyncPeriod = SyncPeriod.All, RootFolder = "PhaScenarios"
        });

        var cheds = new[] 
        {
            "CHEDA.GB.2024.4792831",
            "CHEDD.GB.2024.5019877",
            "CHEDP.GB.2024.4144842",
            "CHEDPP.GB.2024.3726460",
            "CHEDA.GB.2024.5129502",
            "CHEDP.GB.2024.5137241",
            "CHEDD.GB.2024.5226413",
            "CHEDD.GB.2024.5351769",
            "CHEDD.GB.2024.5118377",
            "CHEDP.GB.2024.5093007",
            "CHEDP.GB.2024.5328437",
            "CHEDP.GB.2024.5249145",
            "CHEDP.GB.2024.5140733",
            "CHEDP.GB.2024.5192091",
            "CHEDP.GB.2024.5270338"
        };

        var document = Client.AsJsonApiClient().Get("api/import-notifications?page[size]=20");
        document.Data.Count.Should().Be(cheds.Length);
        
        if (_saveData)
        {
            const string outputDirectory = "../../../PhaScenarioTestsOutput";
            Directory.CreateDirectory(outputDirectory);
            
            foreach (var ched in cheds)
            {
                var json = await GetDocument($"api/import-notifications/{ched}", Client.AsHttpClient());
                await File.WriteAllTextAsync($"{outputDirectory}/btms-import-notification-single-{ched}.json", json);
            }

            var mrns = new[]
            {
                "24GBE0XBAS7Z0J5AR0",
                "24GBDFPD8F0ECL9AR8",
                "24GBDFPJ28XLD8OAR5",
                "24GBD8ZE70M2T3ZAR1",
                "24GBEGTIWJ6LOYWAR9",
                "24GBE2JU5B0W5Z0AR8",
                "24GBCUESXSEE3WQAR6",
                "24GBE6SLBBGLL33AR6",
                "24GBDMPA1EV781WAR7",
                "24GBCUDNXBN1JNRAR5"
            };

            foreach (var mrn in mrns)
            {
                var json = await GetDocument($"api/movements/{mrn}", Client.AsHttpClient());
                await File.WriteAllTextAsync($"{outputDirectory}/btms-movement-single-{mrn}.json", json);
            }
        }
    }

    private static async Task<string> GetDocument(string path, HttpClient httpClient)
    {
        var response = await httpClient.GetAsync(path);
        response.EnsureSuccessStatusCode();

        var json1 = await response.Content.ReadAsStringAsync();
        var document = JsonDocument.Parse(json1);
#pragma warning disable CA1869
        json1 = JsonSerializer.Serialize(document, new JsonSerializerOptions { WriteIndented = true });
#pragma warning restore CA1869
        return json1;
    }
}