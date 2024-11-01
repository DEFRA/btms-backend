using System.IO;
using System.Net;
using Microsoft.Extensions.Logging;

namespace Cdms.BlobService
{
    public class Status
    {
        public string Description { get; set; } = default!;

        public bool Success { get; set; }
    }
}