using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Cdms.Business.Commands;
using Cdms.SyncJob;
using CdmsBackend.IntegrationTests.Helpers;
using FluentAssertions;
using idunno.Authentication.Basic;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;
using Xunit.Abstractions;
[assembly: CollectionBehavior(DisableTestParallelization = true)]

namespace CdmsBackend.IntegrationTests
{
    public abstract class BaseApiTests
    {
        protected readonly HttpClient Client;
        protected readonly IntegrationTestsApplicationFactory Factory;

        protected BaseApiTests(IntegrationTestsApplicationFactory factory, ITestOutputHelper testOutputHelper)
        {
            this.Factory = factory;
            this.Factory.TestOutputHelper = testOutputHelper;
            this.Factory.DatabaseName = "SmokeTests";
            Client =
                this.Factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
            string credentials = "IntTest:Password";
            byte[] credentialsAsBytes = Encoding.UTF8.GetBytes(credentials.ToCharArray());
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
                SyncJobStatus status = SyncJobStatus.Pending;

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
}

public class SyncJobResponse
{
    public SyncJobStatus Status { get; set; }
}