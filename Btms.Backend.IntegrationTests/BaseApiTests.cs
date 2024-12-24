using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Btms.Backend.IntegrationTests.Helpers;
using Btms.Business.Commands;
using Btms.SyncJob;
using FluentAssertions;
using idunno.Authentication.Basic;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;
using Xunit.Abstractions;
[assembly: CollectionBehavior(DisableTestParallelization = true)]

namespace Btms.Backend.IntegrationTests;

public abstract class BaseApiTests
{
    protected readonly HttpClient Client;
    internal readonly IIntegrationTestsApplicationFactory Factory;
    // protected readonly IntegrationTestsApplicationFactory Factory;

    protected async Task ClearDb()
    {
        await Factory.ClearDb(Client);
    }
    protected BaseApiTests(IIntegrationTestsApplicationFactory factory, ITestOutputHelper testOutputHelper)
    {
        Factory = factory;
        Factory.TestOutputHelper = testOutputHelper;
        Factory.DatabaseName = "SmokeTests";
        Client =
            Factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
        var credentials = "IntTest:Password";
        var credentialsAsBytes = Encoding.UTF8.GetBytes(credentials.ToCharArray());
        var encodedCredentials = Convert.ToBase64String(credentialsAsBytes);
        Client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue(BasicAuthenticationDefaults.AuthenticationScheme, encodedCredentials);
    }

    private async Task WaitOnJobCompleting(Uri jobUri)
    {
        var jsonOptions = new JsonSerializerOptions();
        jsonOptions.Converters.Add(new JsonStringEnumConverter());
        jsonOptions.PropertyNameCaseInsensitive = true;

        var jobStatusTask = Task.Run(async () =>
        {
            var status = SyncJobStatus.Pending;

            while (status != SyncJobStatus.Completed)
            {
                await Task.Delay(200);
                var jobResponse = await Client.GetAsync(jobUri);
                var syncJob = await jobResponse.Content.ReadFromJsonAsync<SyncJobResponse>(jsonOptions);
                if (syncJob != null) status = syncJob.Status;
            }
        });

        var winningTask = await Task.WhenAny(
            jobStatusTask,
            Task.Delay(TimeSpan.FromMinutes(5)));

        if (winningTask != jobStatusTask)
        {
            Assert.Fail("Waiting for job to complete timed out!");
        }

    }

    protected Task<HttpResponseMessage> MakeSyncDecisionsRequest(SyncDecisionsCommand command)
    {
        return PostCommand(command, "/sync/decisions");
    }

    protected Task<HttpResponseMessage> MakeSyncNotificationsRequest(SyncNotificationsCommand command)
    {
        return PostCommand(command, "/sync/import-notifications");
    }

    protected Task<HttpResponseMessage> MakeSyncClearanceRequest(SyncClearanceRequestsCommand command)
    {
        return PostCommand(command, "/sync/clearance-requests");
    }

    protected Task<HttpResponseMessage> MakeSyncGmrsRequest(SyncGmrsCommand command)
    {
        return PostCommand(command, "/sync/gmrs");

    }

    private async Task<HttpResponseMessage> PostCommand<T>(T command, string uri)
    {
        var jsonData = JsonSerializer.Serialize(command);
        HttpContent content = new StringContent(jsonData, Encoding.UTF8, "application/json");

        //Act
        var response = await Client.PostAsync(uri, content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Accepted);

        //get job id and wait for job to be completed
        var jobUri = response.Headers.Location;
        if (jobUri != null) await WaitOnJobCompleting(jobUri);

        return response;
    }
}