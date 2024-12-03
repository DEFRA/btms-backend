namespace Btms.Backend.Authentication
{
	public interface IClientCredentialsManager
	{
		Task<bool> IsValid(string clientId, string clientSecret);
	}
}