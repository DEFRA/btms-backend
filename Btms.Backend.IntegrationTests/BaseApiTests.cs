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
    internal readonly IIntegrationTestsFixture _fixture;
    // protected readonly IntegrationTestsApplicationFactory Factory;
    
    // protected async Task ClearDb()
    // {
    //     await Client.ClearDb();
    // }
    public BaseApiTests(IIntegrationTestsFixture fixture, ITestOutputHelper testOutputHelper, string databaseName = "SmokeTests")
    {
        _fixture = fixture;
        _fixture.TestOutputHelper = testOutputHelper;
        _fixture.DatabaseName = databaseName;
        Client = _fixture.BtmsClient!;
        
            // _fixture.CreateBtmsClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
        
    }

}