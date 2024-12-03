using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using Azure;

namespace Btms.Backend.IntegrationTests.JsonApiClient
{
    public static class JsonApiClientExtensions
    {
        public static JsonApiClient AsJsonApiClient(this HttpClient client)
        {
            return new JsonApiClient(client);
        }
    }
}