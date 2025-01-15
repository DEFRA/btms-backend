using Btms.Model;
using Btms.Model.Ipaffs;
// using Btms.Types.Ipaffs;
using TestGenerator.IntegrationTesting.Backend.Fixtures;

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
    
    public static ImportNotification GetSingleImportNotification(this BtmsClient client)
    {
        return client.AsJsonApiClient()
            .Get("api/import-notifications")
            .GetResourceObjects<ImportNotification>()
            .Single();
    }
}