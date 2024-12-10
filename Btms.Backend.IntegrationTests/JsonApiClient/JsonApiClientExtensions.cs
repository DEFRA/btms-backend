namespace Btms.Backend.IntegrationTests.JsonApiClient;

public static class JsonApiClientExtensions
{
    public static JsonApiClient AsJsonApiClient(this HttpClient client)
    {
        return new JsonApiClient(client);
    }
}