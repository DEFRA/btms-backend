using FluentAssertions;
using JsonApiDotNetCore.Serialization.JsonConverters;
using Microsoft.AspNetCore.WebUtilities;
using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace TestGenerator.IntegrationTesting.Backend.JsonApiClient;

public class JsonApiClient(HttpClient client)
{
    private static readonly string StringContentType = "application/vnd.api+json";

    private static readonly MediaTypeWithQualityHeaderValue ContentType = new(StringContentType);


    public ManyItemsJsonApiDocument Get(
        string path,
        Dictionary<string, string>? query = null,
        Dictionary<string, string>? headers = null,
        IList<string>? relations = null,
        bool assertStatusCode = true)
    {
        client.DefaultRequestHeaders.Accept.Add(ContentType);

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

        var uri = $"/{path}";

        if (relations != null)
        {
            uri = relations.Aggregate(uri, (current, relation) => $"{current}/{relation}");
        }

        if (query != null)
        {
            uri = QueryHelpers.AddQueryString(uri, query!);
        }

        var responseMessage = client.GetAsync(uri).Result;

        if (assertStatusCode)
            responseMessage.StatusCode.Should().Be(HttpStatusCode.OK, $"Status code was {responseMessage.StatusCode}");
        
        var s = responseMessage.Content.ReadAsStringAsync().Result;

        return JsonSerializer.Deserialize<ManyItemsJsonApiDocument>(s,
            new JsonSerializerOptions
            {
                Converters = { new SingleOrManyDataConverterFactory() },
                PropertyNameCaseInsensitive = true
            })!;
    }

    public SingleItemJsonApiDocument GetById(string id,
        string path,
        Dictionary<string, string>? query = null,
        Dictionary<string, string>? headers = null,
        IList<string>? relations = null)
    {
        client.DefaultRequestHeaders.Accept.Add(ContentType);

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

        var uri = $"/{path}/{id}";

        if (relations != null)
        {
            uri = relations.Aggregate(uri, (current, relation) => $"{current}/{relation}");
        }

        if (query != null)
        {
            uri = QueryHelpers.AddQueryString(uri, query!);
        }

        var responseMessage = client.GetAsync(uri).Result;

        responseMessage.StatusCode.Should().Be(HttpStatusCode.OK, $"Status code was {responseMessage.StatusCode}");

        var s = responseMessage.Content.ReadAsStringAsync().Result;

        return JsonSerializer.Deserialize<SingleItemJsonApiDocument>(s,
            new JsonSerializerOptions
            {
                Converters = { new SingleOrManyDataConverterFactory() },
                PropertyNameCaseInsensitive = true
            })!;
    }
}