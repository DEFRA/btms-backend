using CdmsBackend.Config;
using Microsoft.Extensions.Options;

namespace CdmsBackend.Authentication
{
	public class ClientCredentialsManager(IOptions<ApiOptions> options) : IClientCredentialsManager
	{
		public Task<bool> IsValid(string clientId, string clientSecret)
		{
			if (options.Value.Credentials.TryGetValue(clientId, out string? secret))
			{
				return Task.FromResult(!string.IsNullOrEmpty(secret) && clientSecret.Equals(secret));
			}

			return Task.FromResult(false);
		}
	}
}