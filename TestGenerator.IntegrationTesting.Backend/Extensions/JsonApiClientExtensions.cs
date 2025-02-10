using Btms.Model;
using Btms.Model.Ipaffs;
// using Btms.Types.Ipaffs;
using TestGenerator.IntegrationTesting.Backend.Fixtures;
using TestGenerator.IntegrationTesting.Backend.JsonApiClient;

namespace TestGenerator.IntegrationTesting.Backend.Extensions;

public static class JsonClientExtensions
{
    public static Movement GetSingleMovement(this BtmsClient client)
    {
        return client.AsJsonApiClient()
            .Get("api/movements")
            .GetResourceObjects<Movement>()
            .Single();
    }

    public static SingleItemJsonApiDocument GetMovementByMrn(this BtmsClient client, string mrn)
    {
        return client.AsJsonApiClient()
            .GetById(mrn, "api/movements");
    }

    public static ImportNotification GetFirstImportNotification(this BtmsClient client)
    {
        var results = client.AsJsonApiClient()
            .Get("api/import-notifications")
            .GetResourceObjects<ImportNotification>();
            
        return results.First();
    }

    public static SingleItemJsonApiDocument GetNotificationById(this BtmsClient client, string id)
    {
        return client.AsJsonApiClient()
            .GetById(id, "api/import-notifications");
    }

    public static ImportNotification GetSingleImportNotification(this BtmsClient client)
    {
        return client.AsJsonApiClient()
            .Get("api/import-notifications")
            .GetResourceObjects<ImportNotification>()
            .Single();
    }
}