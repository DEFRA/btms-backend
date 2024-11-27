namespace CdmsBackend.Authentication
{
	public interface IClientCredentialsManager
	{
		Task<bool> IsValid(string clientId, string clientSecret);
	}
}