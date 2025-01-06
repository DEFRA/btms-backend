using Btms.Model;
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
}