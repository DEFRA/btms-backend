using System.Text.RegularExpressions;
using Btms.Common.Extensions;

namespace Btms.Model;

public static partial class RegularExpressions
{
    [GeneratedRegex("(CHEDD|CHEDA|CHEDP|CHEDPP)\\.?GB\\.?(20|21)\\d{2}\\.?\\d{7}[rv]?", RegexOptions.Compiled | RegexOptions.IgnoreCase)]
    internal static partial Regex IPaffsIdentifier();

    [GeneratedRegex("[gbchdv]{5}\\.?(20|21)?\\d{2}\\.?\\d{7}[rv]?", RegexOptions.Compiled | RegexOptions.IgnoreCase)]
    internal static partial Regex DocumentReferenceWithoutCountry();

    [GeneratedRegex("(GBIUU|IUU)\\.*", RegexOptions.Compiled | RegexOptions.IgnoreCase)]
    internal static partial Regex IuuIdentifier();

    [GeneratedRegex("\\d{2,4}\\.?\\d{7}", RegexOptions.Compiled | RegexOptions.IgnoreCase)]
    internal static partial Regex DocumentReferenceIdentifier();

    public static bool IsExactMatch(this Regex regex, string input)
    {
        return regex.Match(input).Value == input;
    }
}


public struct MatchIdentifier(string identifier)
{
    private static string[] _validDocumentCodes = new[] { "C640", "C678", "N853", "N851", "9115", "C085", "N002" };
    public string Identifier { get; private set; } = identifier;

    public string AsCdsDocumentReference()
    {
        // TODO - transfer over from TDM POC
        return $"GBCHD2024{Identifier}";
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
            if (identifier.Length > 7)
            {
                identifier = identifier.Substring(identifier.Length - 7);
            }
            return new MatchIdentifier(identifier);
        }

        throw new FormatException($"Ipaffs Reference invalid format {reference}");
    }

    public static MatchIdentifier FromCds(string reference, string documentCode)
    {
        if (_validDocumentCodes.Contains(documentCode))
        {
            var identifier = RegularExpressions.DocumentReferenceIdentifier().Match(reference).Value.Replace(".", "");

            if (string.IsNullOrEmpty(identifier))
            {
                throw new FormatException($"Document Reference invalid format {reference}");
            }

            if (identifier.Length > 7)
            {
                identifier = identifier.Substring(identifier.Length - 7);
            }
            return new MatchIdentifier(identifier);
        }

        throw new FormatException($"Document Reference invalid format {reference}");
    }

    public static bool TryFromCds(string reference, string documentCode, out MatchIdentifier matchIdentifier)
    {
        try
        {
            matchIdentifier = FromCds(reference, documentCode);
            return true;
        }
        catch (Exception)
        {
            matchIdentifier = default;
            return false;
        }
    }

    public static bool IsValid(string? reference, string? documentCode)
    {
        return reference.HasValue() && documentCode.HasValue() && TryFromCds(reference, documentCode, out var _);
    }

    public static bool IsIuuRef(string? reference)
    {
        return reference.HasValue() && RegularExpressions.IuuIdentifier().IsExactMatch(reference);
    }
}