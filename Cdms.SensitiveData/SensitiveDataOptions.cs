using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;

namespace Cdms.SensitiveData;

public interface ISensitiveDataOptions
{
    public bool Include { get; }

    public Func<string, string> Getter { get;  }
}

public class SensitiveDataOptions : ISensitiveDataOptions
{
    private SensitiveDataOptions()
    {

    }

    public SensitiveDataOptions(IConfiguration configuration)
    {
        Include = configuration.GetValue<bool>("INGEST_INCLUDE_SENSITIVE_DATA");
    }
    public bool Include { get; set; }

    public Func<string, string> Getter { get; set; } = Sha256;

    static string Sha256(string input)
    {
        var crypt = new SHA256Managed();
        string hash = String.Empty;
        byte[] crypto = crypt.ComputeHash(Encoding.ASCII.GetBytes(input));
        foreach (byte theByte in crypto)
        {
            hash += theByte.ToString("x2");
        }
        return hash;
    }

    public static SensitiveDataOptions Default => new ();

    public static SensitiveDataOptions WithSensitiveData => new() { Include = true };
}