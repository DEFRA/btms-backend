namespace Btms.Common.Enum;

public static class EnumExtensions
{
    private static readonly Dictionary<string, object> converters =
        new Dictionary<string, object>();

    public static string GetValue<TEnum>(this TEnum e) where TEnum : struct, System.Enum
    {
        var key = e.GetType()!.FullName!;
        if (!converters.TryGetValue(key, out var enumObject))
        {
            enumObject = new JsonStringEnumConverterEx<TEnum>();
            converters.Add(key, enumObject);
        }

        var enumLookup = (JsonStringEnumConverterEx<TEnum>)enumObject;

        return enumLookup.GetValue(e);
    }

    public static bool IsAny<T>(this T e, params T[] args) where T : struct, IComparable
    {
        return args.Any(a => e.Equals(a));
    }
}