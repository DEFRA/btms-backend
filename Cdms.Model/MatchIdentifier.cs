using Cdms.Model.Ipaffs;

namespace Cdms.Model;

public struct MatchIdentifier(int identifier)
{
    public int Identifier { get; private set; } = identifier;

    public string AsCdsDocumentReference()
    {
        // TODO - transfer over from TDM POC
        return $"GBCHD2024.{Identifier}";
    }
    
    public static MatchIdentifier FromNotification(string reference)
    {
        if (reference == null)
        {
            throw new ArgumentNullException(nameof(reference));
        }

        var parts = reference.Split(".");
        int identifier;
        if (char.IsDigit(parts[3].Last()))
        {
            identifier = int.Parse(parts[3]);
        }
        else
        {
            identifier = int.Parse(parts[3].Remove(parts[3].Length - 1));
        }

        return new MatchIdentifier(identifier);
    }

    public static MatchIdentifier FromCds(string reference)
    {
        int identifier;
        var parts = reference.Split(".");

        var identifierString = parts[^1];
        if (char.IsDigit(identifierString.Last()))
        {
            identifier = int.Parse(identifierString);
        }
        else
        {
            identifier = int.Parse(identifierString.Remove(identifierString.Length - 1));
        }

        return new MatchIdentifier(identifier);
    }
}