using System.Net.Http.Headers;
using System.Text.Json;
using JsonApiDotNetCore.Serialization.JsonConverters;
using JsonApiSerializer;
using Microsoft.AspNetCore.WebUtilities;
using ErrorEventArgs = Newtonsoft.Json.Serialization.ErrorEventArgs;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace CdmsBackend.IntegrationTests.JsonApiClient;

public class JsonApiClient(HttpClient client)
{
    static string strContentType = "application/vnd.api+json";

    static MediaTypeWithQualityHeaderValue contentType = new MediaTypeWithQualityHeaderValue(strContentType);


    public ManyItemsJsonApiDocument Get(
        string path,
        Dictionary<string, string> query = null,
        Dictionary<string, string> headers = null,
        IList<string> relations = null)
    {
        client.DefaultRequestHeaders.Accept.Add(contentType);

        if (headers != null)
        {
            foreach (var header in headers)
            {
                if (!string.IsNullOrWhiteSpace(header.Value))
                {
                    client.DefaultRequestHeaders.Add(header.Key, header.Value);
                }
            }
        }

        string uri = $"/{path}";

        if (relations != null)
        {
            uri = relations.Aggregate(uri, (current, relation) => $"{current}/{relation}");
        }

        if (query != null)
        {
            uri = QueryHelpers.AddQueryString(uri, query);
        }

        HttpResponseMessage responseMessage = client.GetAsync(uri).Result;

        var s = responseMessage.Content.ReadAsStringAsync().Result;

        return JsonSerializer.Deserialize<ManyItemsJsonApiDocument>(s,
            new JsonSerializerOptions()
            {
                Converters = { new SingleOrManyDataConverterFactory() }, PropertyNameCaseInsensitive = true
            });
    }

    public SingleItemJsonApiDocument GetById(string id,
        string path,
        Dictionary<string, string> query = null,
        Dictionary<string, string> headers = null,
        IList<string> relations = null)
    {
        client.DefaultRequestHeaders.Accept.Add(contentType);

        if (headers != null)
        {
            foreach (var header in headers)
            {
                if (!string.IsNullOrWhiteSpace(header.Value))
                {
                    client.DefaultRequestHeaders.Add(header.Key, header.Value);
                }
            }
        }

        string uri = $"/{path}/{id}";

        if (relations != null)
        {
            uri = relations.Aggregate(uri, (current, relation) => $"{current}/{relation}");
        }

        if (query != null)
        {
            uri = QueryHelpers.AddQueryString(uri, query);
        }

        HttpResponseMessage responseMessage = client.GetAsync(uri).Result;

        var s = responseMessage.Content.ReadAsStringAsync().Result;

        return JsonSerializer.Deserialize<SingleItemJsonApiDocument>(s,
            new JsonSerializerOptions()
            {
                Converters = { new SingleOrManyDataConverterFactory() }, PropertyNameCaseInsensitive = true
            });
    }
}