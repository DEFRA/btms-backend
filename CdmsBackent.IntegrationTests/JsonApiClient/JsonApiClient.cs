using System.Net.Http.Headers;
using JsonApiSerializer;
using JsonApiSerializer.JsonApi;
using Microsoft.AspNetCore.WebUtilities;
using Newtonsoft.Json;
using ErrorEventArgs = Newtonsoft.Json.Serialization.ErrorEventArgs;

namespace CdmsBackend.IntegrationTests.JsonApiClient;

public class JsonApiClient(HttpClient client)
{
    static string strContentType = "application/vnd.api+json";

    static MediaTypeWithQualityHeaderValue contentType = new MediaTypeWithQualityHeaderValue(strContentType);

    public static void HandleDeserializationError(object sender, ErrorEventArgs errorArgs)
    {
        errorArgs.ErrorContext.Handled = true;
    }

    /// <summary>
    /// Jsonapi serializer settings.
    /// </summary>
    static JsonApiSerializerSettings settings = new JsonApiSerializerSettings() { Error = HandleDeserializationError, };

    public Response<TRequest[]> Get<TRequest>(
        string path,
        Dictionary<string, string> query = null,
        Dictionary<string, string> headers = null,
        IList<string> relations = null) where TRequest : class, new()
    {
        var response = new Response<TRequest[]>();

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

        response.HttpStatusCode = responseMessage.StatusCode;

        response.IsSuccess = responseMessage.IsSuccessStatusCode;

        var s = responseMessage.Content.ReadAsStringAsync().Result;

        if (responseMessage.IsSuccessStatusCode)
        {
            response.DocumentRoot =
                JsonConvert.DeserializeObject<DocumentRoot<TRequest[]>>(
                    responseMessage.Content.ReadAsStringAsync().Result, settings);
        }
        else
        {
            response.Error =
                JsonConvert.DeserializeObject<Error>(responseMessage.Content.ReadAsStringAsync().Result, settings);
        }

        return response;
    }

    public Response<TRequest> GetById<TRequest>(string id,
        string path,
        Dictionary<string, string> query = null,
        Dictionary<string, string> headers = null,
        IList<string> relations = null) where TRequest : class, new()
    {
        var response = new Response<TRequest>();


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

        response.HttpStatusCode = responseMessage.StatusCode;

        response.IsSuccess = responseMessage.IsSuccessStatusCode;

        if (responseMessage.IsSuccessStatusCode)
        {
            response.DocumentRoot =
                JsonConvert.DeserializeObject<DocumentRoot<TRequest>>(
                    responseMessage.Content.ReadAsStringAsync().Result, settings);
        }
        else
        {
            response.Error =
                JsonConvert.DeserializeObject<Error>(responseMessage.Content.ReadAsStringAsync().Result, settings);
        }


        return response;
    }
}