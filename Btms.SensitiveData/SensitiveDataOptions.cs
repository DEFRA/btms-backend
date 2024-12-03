using System.Security.Cryptography;
using System.Text;

namespace Btms.SensitiveData;

public class SensitiveDataOptions
{
    public const string SectionName = nameof(SensitiveDataOptions);
    public bool Include { get; set; }

    public Func<string, string> Getter { get; set; } = Sha256;

    static string Sha256(string input)
    {
        var crypt = SHA256.Create();
        StringBuilder hash = new StringBuilder();
        byte[] crypto = crypt.ComputeHash(Encoding.ASCII.GetBytes(input));
        foreach (byte theByte in crypto)
        {
            hash.Append(theByte.ToString("x2"));
        }

        return hash.ToString();
    }

    public static SensitiveDataOptions Default => new();

    public static SensitiveDataOptions WithSensitiveData => new() { Include = true };
}