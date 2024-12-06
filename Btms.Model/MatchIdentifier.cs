using Btms.Model.Ipaffs;

namespace Btms.Model;

public struct MatchIdentifier(string identifier)
{
    public string Identifier { get; private set; } = identifier;

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
        string identifier;
        if (char.IsDigit(parts[3].Last()))
        {
            identifier = parts[3];
        }
        else
        {
            identifier = parts[3].Remove(parts[3].Length - 1);
        }

        return new MatchIdentifier(identifier);
    }

    public static MatchIdentifier FromCds(string reference)
    {
        string identifier;
        var parts = reference.Split(".");

        var identifierString = parts[^1];
        if (char.IsDigit(identifierString.Last()))
        {
            identifier = identifierString;
        }
        else
        {
            identifier = identifierString.Remove(identifierString.Length - 1);
        }

        return new MatchIdentifier(identifier);
    }

    public static bool TryFromCds(string reference, out MatchIdentifier matchIdentifier)
    {
        try
        {
            matchIdentifier = MatchIdentifier.FromCds(reference);
            return true;
        }
        catch (Exception)
        {
            matchIdentifier = default;
            return false;
        }
    }
}