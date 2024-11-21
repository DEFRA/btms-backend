namespace CdmsBackend.Cli;

public static class PascalCaseNamingPolicy
{
    public static string ConvertName(string name)
    {
        if (string.IsNullOrEmpty(name) || !char.IsLower(name[0]))
        {
            return name;
        }

        return string.Create(name.Length, name, (chars, value) =>
        {
            value.CopyTo(chars);
            FixCasing(chars);
        });
    }

    private static void FixCasing(Span<char> chars)
    {
        chars[0] = char.ToUpperInvariant(chars[0]);
    }
}