
using Cdms.BlobService;
using Cdms.Common.Extensions;
using CdmsBackend.Config;
using Microsoft.Extensions.Options;

namespace CdmsBackend.Endpoints;

public static class DiagnosticEndpoints
{
    private const string BaseRoute = "diagnostics";

    public static void UseDiagnosticEndpoints(this IEndpointRouteBuilder app, IOptions<ApiOptions> options)
    {
        if (options.Value.EnableManagement)
        {
            app.MapGet(BaseRoute + "/blob", GetBlobDiagnosticAsync).AllowAnonymous();
        }
    }
    
    private static async Task<IResult> GetBlobDiagnosticAsync(
        IBlobService service)
    {
        var result = await service.CheckBlobAsync();
        Console.WriteLine(result.ToJson());
        if (result.Success)
        {
            return Results.Ok(result);    
        }
        return Results.Conflict(result);
    }
}