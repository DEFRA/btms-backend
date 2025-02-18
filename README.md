# BTMS Backend

Core delivery C# ASP.NET backend template.

* [MongoDB](#mongodb)
* [Testing](#testing)
* [Running](#running)
* [Test Data](#test-data)

# MongoDB

Run MongoDB as a container. See btms-frontend repository for necessary compose configuration.

# Testing

If running integration tests, start the MongoDB dependencies as noted above.

Also start the Azure Service Bus emulator included in the compose.yml file.

All tests within the solution should then run locally.

# Running

Open the btms-backend solution and run the Btms.Backend profile.


# Test Data
We are able to obtain test data to use in our tests a few ways.

## Canned test data
This is data that we created by hand based on examples of messages that we had obtained. Canned test data can be found in [`Btms.Backend.IntegrationTests/Fixtures/SmokeTest`](Btms.Backend.IntegrationTests/Fixtures/SmokeTest). In there you will find relevant folders for the messages that you want to simulate:
* ALVS - Custom Record notifications from CDS
* DECISIONS - Decision that is made by BTMS.
* GVMSAPIRESPONSE - ??
* IPAFFS - CHED notifications from IPAFFS
  * CHEDA
  * CHEDD
  * CHEDP
  * CHEDPP

## Test Data Generator Scenarios
The Test Data Generator can be found in the `tools` project ([`tools/TestDataGenerator`](TestDataGenerator/TestDataGenerator.csproj)). The test data is generated based on specifications provided in a scenario e.g. [`ChedASimpleMatchScenarioGenerator.cs`](TestDataGenerator/Scenarios/ChedASimpleMatchScenarioGenerator.cs). A scenario should container at least a `GetNotificationBuilder` or `GetClearanceRequestBuilder`.

Example usage of `GetNotificationBuilder`
```csharp
var notification = GetNotificationBuilder("cheda-one-commodity")
    .WithCreationDate(entryDate)
    .WithRandomArrivalDateTime(config.ArrivalDateRange)
    .WithReferenceNumber(ImportNotificationTypeEnum.Cveda, scenario, entryDate, item)
    .ValidateAndBuild();
```

Example usage of `GetClearanceRequestBuilder`
```csharp
var clearanceRequest = GetClearanceRequestBuilder("cr-one-item")
    .WithCreationDate(entryDate)
    .WithArrivalDateTimeOffset(notification.PartOne!.ArrivalDate, notification.PartOne!.ArrivalTime)
    .WithReferenceNumber(notification.ReferenceNumber!)
    .ValidateAndBuild();
```
Note: 
* Both the Notification Builder and Clearance Request Builder both take a sample file which it uses as a basis to create the test data. The sample file is located in the btms-test-date repository in the [`Samples` folder](https://github.com/DEFRA/btms-test-data). 

After creating your scenario your will need to add it to `ConfigureTestGenerationServices` in [`BuilderExtensions.cs`](TestDataGenerator/Helpers/BuilderExtensions.cs). 

## Using scen

## Generating datasets from scenarios

A data set is a folder of files that simulates the data lake as a source of larger volumes of scenarios. Note, the dataset mechanism invokes each ScenarioGenerator multiple times, as specified in the configuration. Some scenarios use a single session of redacted data unmodified and this would just result in lots of messages for the same resource. To perform correctly in a dataset a scenario must be coded to generate unique IDs for each invocation of the ScenarioGenerator.Generate abstract method.

You can create a dataset in [`Datasets.cs`](TestDataGenerator/Config/Datasets.cs).

Example dataset:

```csharp
 var datasets = new[]
    {
        new
        {
            Dataset = "All-CHED-No-Match",
            RootPath = "GENERATED-ALL-CHED-NO-MATCH",
            Scenarios = new[] { app.CreateScenarioConfig<AllChedsNoMatchScenarioGenerator>(1, 1) }
        },
        ...
    }
```

* Dataset - Name of the dataset 
* RootPath - Folder where the data will be created in. The folder will be in [`TestDataGenerator/.test-data-generator`](TestDataGenerator/.test-data-generator). 
* Scenarios - List of scenarios to create test data for. The CreateScenarioConfig generates scenarios based on the Scenario types from the [`Scenarios`](TestDataGenerator/Scenarios) folder.

And finally, in order to regenerate a dataset, can add configuration to [`Properties/launchSettings.json`](TestDataGenerator/Properties/launchSettings.json):
```json
{
    "profiles": {
        "Generate All CHED no match": {
            "commandName": "Project",
            "commandLineArgs": "All-CHED-No-Match",
            "environmentVariables": {
                "DMP_ENVIRONMENT": "dev",
                "DMP_SERVICE_BUS_NAME": "DEVTREINFSB1001",
                "DMP_BLOB_STORAGE_NAME": "devdmpinfdl1001",
                "DMP_SLOT": "1003",
                "AZURE_TENANT_ID": "c9d74090-b4e6-4b04-981d-e6757a160812"
            }
        }
    }
}
```

* Give the profile a name. This can be free text.
* commandLineArgs - The name of the new dataset.
* The rest of the configuration can be copied from the other profiles.

### Creating new sample file
You may want to a new sample if you are creating data for a new scenario. To do this:
* Place the new sample file with the relevant JSON in the [`Samples`](TestDataGenerator/Scenarios/Samples) folder.
* Change the Properties of the file in Rider:
  * Build action: **Content**
  * Copy to output directory: **Copy if newer**

## Using data from Blob Storage
We have imported redacted production data that is stored in a Blob Storage. We can use BTMS Backend to import this data.

* Update [`local.env`](Btms.Backend/Properties/local.env):
  * For one day dataset - `BusinessOptions:DmpBlobRootFolder=PRODREDACTED-20241204`
  * For one month dataset - `BusinessOptions:DmpBlobRootFolder=PRODREDACTED-202411`
  * When importing the data set the following:
    ```dotenv
    BlobServiceOptions:CacheWriteEnabled=true
    # BlobServiceOptions:CacheReadEnabled=true
    ```
  * After importing update the config to be the following so the data doesn't get imported again when you call the `initialise` API (http://0.0.0.0:5002/mgmt/initialise?syncPeriod=All)
    ```dotenv
    # BlobServiceOptions:CacheWriteEnabled=true
    BlobServiceOptions:CacheReadEnabled=true
    ```
  * Once the config has been updated, start BTMS Backend and call the `initialise` API (http://0.0.0.0:5002/mgmt/initialise?syncPeriod=All). Note that a large amount of data will be loaded, particularly the full month dataset. It is also advisable to run Backend from a standalone terminal rather than from Rider as it struggles running this task.
      

