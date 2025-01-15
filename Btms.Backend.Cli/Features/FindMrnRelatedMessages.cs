using CommandLine;
using MediatR;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Btms.Backend.Cli.Features;

public class HistoryResponse()
{
    [JsonInclude]
    public required List<HistoryEntry> Items;
};

public record class HistoryEntry(string resourceType, string resourceId);

[Verb("find-mrn-related-messages", isDefault: false, HelpText = "Finds messages from data lake for MRN and related CHEDs")]
internal class FindMrnRelatedMessagesCommand : IRequest
{
    [Option('m', "mrn", Required = true,
        HelpText = "The reference of the MRN to start with.")]
    public string Mrn { get; set; } = null!;

    [Option('a', "api", Required = false,
        HelpText = "The BTMS Backend API to query.")]
    public string Api { get; set; } = "http://btms-backend.localtest.me:5002";
    
    public class Handler : AsyncRequestHandler<FindMrnRelatedMessagesCommand>
    {
        protected override async Task<string> Handle(FindMrnRelatedMessagesCommand request,
            CancellationToken cancellationToken)
        {
            Console.WriteLine("Getting Messages from Blob Storage for MRN {0}", request.Mrn);
            Console.WriteLine("Requesting history from {0}", request.Api);

            var url = $"{request.Api}/analytics/timeline?movementId={request.Mrn}";
            
            Console.WriteLine("API Call to {0}", url);
            
            using HttpClient client = new();
            
            await using Stream stream =
                await client.GetStreamAsync(url);
            
            var historyResponseItems =
                (await JsonSerializer.DeserializeAsync<HistoryResponse>(stream, new JsonSerializerOptions()
                {
                    PropertyNameCaseInsensitive = true
                }))!
                .Items
                .Distinct()
                .ToList();
            
            Console.WriteLine("{0} items found", historyResponseItems.Count);
            // historyResponse.Items.First().
            return await Task.FromResult("FindMrnRelatedMessagesCommand");
        }
    }
}