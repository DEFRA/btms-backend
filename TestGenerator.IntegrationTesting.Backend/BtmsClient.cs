using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Btms.Business.Commands;
using Btms.Model.Ipaffs;
using Btms.SyncJob;
using Elastic.CommonSchema;
using FluentAssertions;
using idunno.Authentication.Basic;
using Microsoft.Extensions.Logging;
using Xunit;

namespace TestGenerator.IntegrationTesting.Backend.Fixtures;

public class BtmsClient
{
    private HttpClient client;

    public BtmsClient(HttpClient aclient)
    {
        client = aclient;

        var credentials = "IntTest:Password";
        var credentialsAsBytes = Encoding.UTF8.GetBytes(credentials.ToCharArray());
        var encodedCredentials = Convert.ToBase64String(credentialsAsBytes);
        client.DefaultRequestHeaders.Authorization =
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
                var jobResponse = await client.GetAsync(jobUri);
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

    public Task<HttpResponseMessage> MakeSyncDecisionsRequest(SyncDecisionsCommand command)
    {
        return PostCommand(command, "/sync/decisions");
    }

    public Task<HttpResponseMessage> MakeSyncNotificationsRequest(SyncNotificationsCommand command)
    {
        return PostCommand(command, "/sync/import-notifications");
    }

    public Task<HttpResponseMessage> MakeSyncClearanceRequest(SyncClearanceRequestsCommand command)
    {
        return PostCommand(command, "/sync/clearance-requests");
    }

    public Task<HttpResponseMessage> MakeSyncGmrsRequest(SyncGmrsCommand command)
    {
        return PostCommand(command, "/sync/gmrs");

    }

    public async Task ClearDb()
    {
        await client.GetAsync("mgmt/collections/drop");
    }

    public Task<HttpResponseMessage> GetAnalyticsDashboard()
    {
        return client.GetAsync(
            $"/analytics/dashboard");
    }

    public Task<HttpResponseMessage> CancelJob(string? jobId)
    {
        return client.GetAsync($"/sync/jobs/{jobId}/cancel");
    }

    public Task<HttpResponseMessage> GetJob(string? jobId)
    {
        return client.GetAsync($"/sync/jobs/{jobId}");
    }

    public async Task<(HttpResponseMessage, string?)> StartJob<T>(T command, string uri)
    {
        var jsonData = JsonSerializer.Serialize(command);
        HttpContent content = new StringContent(jsonData, Encoding.UTF8, "application/json");

        var response = await client.PostAsync(uri, content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Accepted);

        return (response, Path.GetFileName(response.Headers.Location?.ToString()));
    }

    public Task<HttpResponseMessage> GetExceptions()
    {
        return client.GetAsync(
            $"/analytics/exceptions");
    }

    public Task<HttpResponseMessage> GetAnalyticsDashboard(string[] charts,
        ImportNotificationTypeEnum[]? chedTypes = null,
        string? country = null,
        DateTime? dateFrom = null,
        DateTime? dateTo = null,
        bool finalisedOnly = false)
    {
        var chartsFilter = String.Join("&chartsToRender=", charts);
        var dateFromFilter = dateFrom.HasValue ? $"&dateFrom={dateFrom.Value:yyyy-MM-dd}" : "";
        var dateToFilter = dateTo.HasValue ? $"&dateTo={dateTo.Value:yyyy-MM-dd}" : "";
        var countryFilter = country != null ? $"&coo={country}" : "";
        var chedTypeFilter = chedTypes != null ? "&chedType=" + String.Join("&chedType=", chedTypes) : "";
        var finalisedOnlyFilter = $"&finalisedOnly={finalisedOnly}";

        return client.GetAsync(
            $"/analytics/dashboard?chartsToRender={chartsFilter}{dateFromFilter}{dateToFilter}{countryFilter}{chedTypeFilter}{finalisedOnlyFilter}");
    }

    private async Task<HttpResponseMessage> PostCommand<T>(T command, string uri)
    {
        var jsonData = JsonSerializer.Serialize(command);
        HttpContent content = new StringContent(jsonData, Encoding.UTF8, "application/json");

        //Act
        var response = await client.PostAsync(uri, content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Accepted);

        //get job id and wait for job to be completed
        var jobUri = response.Headers.Location;
        if (jobUri != null) await WaitOnJobCompleting(jobUri);

        return response;
    }

    public JsonApiClient.JsonApiClient AsJsonApiClient()
    {
        return new JsonApiClient.JsonApiClient(client);
    }

    public HttpClient AsHttpClient() => client;
}