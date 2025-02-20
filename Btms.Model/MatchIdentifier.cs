using System.Text.RegularExpressions;

namespace Btms.Model;

public static partial class RegularExpressions
{
    [GeneratedRegex("(CHEDD|CHEDA|CHEDP|CHEDPP)\\.?GB\\.?(20|21)\\d{2}\\.?\\d{7}[rv]?", RegexOptions.Compiled | RegexOptions.IgnoreCase)]
    internal static partial Regex IPaffsIdentifier();

    [GeneratedRegex("(GBCVD|GBCHD|GBCVD|GBCHD)\\.?(20|21)\\d{2}\\.?\\d{7}[rv]?", RegexOptions.Compiled | RegexOptions.IgnoreCase)]
    internal static partial Regex DocumentReferenceWithoutCountry();

    [GeneratedRegex("\\d{4}\\.?\\d{7}", RegexOptions.Compiled | RegexOptions.IgnoreCase)]
    internal static partial Regex DocumentReferenceIdentifier();

    public static bool IsExactMatch(this Regex regex, string input)
    {
        return regex.Match(input).Value == input;
    }
}


public struct MatchIdentifier(string identifier)
{
    public string Identifier { get; private set; } = identifier;

    public string AsCdsDocumentReference()
    {
        // TODO - transfer over from TDM POC
        return $"GBCHD{Identifier.Substring(0, 4)}.{Identifier.Substring(4)}";
    }

    public static MatchIdentifier FromNotification(string reference)
    {
        if (reference == null)
        {
            throw new ArgumentNullException(nameof(reference));
        }

        if (RegularExpressions.IPaffsIdentifier().IsExactMatch(reference))
        {
            var identifier = RegularExpressions.DocumentReferenceIdentifier().Match(reference).Value.Replace(".", "");
            return new MatchIdentifier(identifier);
        }

        throw new FormatException($"Ipaffs Reference invalid format {reference}");
    }

    public static MatchIdentifier FromCds(string reference)
    {
        if (RegularExpressions.IPaffsIdentifier().IsExactMatch(reference) ||
            RegularExpressions.DocumentReferenceWithoutCountry().IsExactMatch(reference))
        {
            var identifier = RegularExpressions.DocumentReferenceIdentifier().Match(reference).Value.Replace(".", "");
            return new MatchIdentifier(identifier);
        }

        throw new FormatException($"Document Reference invalid format {reference}");
    }

    public static bool TryFromCds(string reference, out MatchIdentifier matchIdentifier)
    {
        try
        {
            matchIdentifier = FromCds(reference);
            return true;
        }
        catch (Exception)
        {
            matchIdentifier = default;
            return false;
        }
    }
}