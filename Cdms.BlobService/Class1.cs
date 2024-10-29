using System.IO;
using System.Net;
using Azure.Storage.Blobs;
using Microsoft.Extensions.Logging;

namespace Cdms.BlobService
{
    public interface IBlobService
    {
        public Task<Status> CheckBlobAsync();
        public Task<Status> CheckBlobAsync(string uri);
        public IAsyncEnumerable<IBlobItem> GetResourcesAsync(string prefix);
        public Task<IBlobItem?> GetBlobAsync(string path);
    }

    public class Status
    {
        public string Description { get; set; } = default!;

        public bool Success { get; set; }
    }

    public interface IBlobItem
    {
        string Name { get; set; }
        string Content { get; set; }

        Task<string> Download();
    }

    public class SynchroniserBlobItem(BlobClient client) : IBlobItem
    {
        public string Name { get; set; } = default!;

        public string NormalisedName { get; set; } = default;
        public string Content { get; set; } = default!;

        public async Task<string> Download()
        {
            var content = await client.DownloadContentAsync();
            Content = content.Value.Content.ToString();
            return Content;
        }
    }
}