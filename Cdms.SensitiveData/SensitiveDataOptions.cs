using System.Security.Cryptography;
using System.Text;

namespace Cdms.SensitiveData;

public class SensitiveDataOptions
{
    public const string SectionName = nameof(SensitiveDataOptions);
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

    public static SensitiveDataOptions Default => new();

    public static SensitiveDataOptions WithSensitiveData => new() { Include = true };
}