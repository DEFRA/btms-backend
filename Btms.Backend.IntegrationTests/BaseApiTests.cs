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
    protected readonly BtmsClient Client;
    internal readonly IIntegrationTestsApplicationFactory Factory;
    // protected readonly IntegrationTestsApplicationFactory Factory;
    
    protected async Task ClearDb()
    {
        await Client.ClearDb();
    }
    protected BaseApiTests(IIntegrationTestsApplicationFactory factory, ITestOutputHelper testOutputHelper, string databaseName = "SmokeTests")
    {
        Factory = factory;
        Factory.TestOutputHelper = testOutputHelper;
        Factory.DatabaseName = databaseName;
        Client =
            Factory.CreateBtmsClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
        
    }

}