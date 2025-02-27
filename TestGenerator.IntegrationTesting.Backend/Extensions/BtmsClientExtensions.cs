using Btms.Common.Extensions;
using Btms.Model;
using Btms.Model.Ipaffs;
using TestGenerator.IntegrationTesting.Backend.Fixtures;
using TestGenerator.IntegrationTesting.Backend.JsonApiClient;

namespace TestGenerator.IntegrationTesting.Backend.Extensions;

public static class BtmsClientExtensions
{
    private const string MovementsEndpoint = "api/movements";
    private const string ImportNotificationsEndpoint = "api/import-notifications";

    public static Movement GetSingleMovement(this BtmsClient client)
    {
        return client.AsJsonApiClient()
            .Get(MovementsEndpoint)
            .GetResourceObjects<Movement>()
            .Single();
    }


    public static SingleItemJsonApiDocument GetMovementByMrn(this BtmsClient client, string mrn)
    {
        return client.AsJsonApiClient()
            .GetById(mrn, MovementsEndpoint);
    }

    public static ImportNotification GetFirstImportNotification(this BtmsClient client)
    {
        var results = client.AsJsonApiClient()
            .Get(ImportNotificationsEndpoint)
            .GetResourceObjects<ImportNotification>();

        return results.First();
    }

    public static SingleItemJsonApiDocument GetNotificationById(this BtmsClient client, string id)
    {
        return client.AsJsonApiClient()
            .GetById(id, ImportNotificationsEndpoint);
    }

    public static ImportNotification GetImportNotificationById(this BtmsClient client, string id)
    {
        return client.AsJsonApiClient()
            .GetById(id, ImportNotificationsEndpoint)
            .GetResourceObject<ImportNotification>();
    }

    public static ImportNotification GetSingleImportNotification(this BtmsClient client)
    {
        return client.AsJsonApiClient()
            .Get(ImportNotificationsEndpoint)
            .GetResourceObjects<ImportNotification>()
            .Single();
    }
}