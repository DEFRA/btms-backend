using Btms.BlobService;
using Btms.Common.Extensions;
using Microsoft.VisualBasic;
using DateTime = System.DateTime;

namespace Btms.Business.Commands;

public static class BlobServiceExtensions
{
    public static IAsyncEnumerable<IBlobItem> GetBlobItems(this IBlobService blobService, string folder, SyncPeriod period, CancellationToken cancellationToken)
    {

        var paths = period.GetPeriodPaths();

        return paths
            .Select((periodPath) =>
                blobService.GetResourcesAsync($"{folder}{periodPath}", cancellationToken)
            )
            .FlattenAsyncEnumerable();

    }
}