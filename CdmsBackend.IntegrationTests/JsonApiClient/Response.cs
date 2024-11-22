using System.Net;
using JsonApiSerializer.JsonApi;

namespace CdmsBackend.IntegrationTests.JsonApiClient;

public class Response<T>
{
    public DocumentRoot<T> DocumentRoot { get; internal set; } = null!;
    public HttpStatusCode HttpStatusCode { get; internal set; }
    public Error Error { get; set; } = null!;
    public bool IsSuccess { get; internal set; }
}