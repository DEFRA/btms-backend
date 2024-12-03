using System.Text.Json;

namespace Cdms.SensitiveData;

public static class SensitiveFieldsProvider
{
    private static Dictionary<Type, List<string>> cache = new();
    private static readonly object cacheLock = new ();
    public static List<string> Get<T>()
    {
        lock (cacheLock)
        {
            if (cache.TryGetValue(typeof(T), out var value))
            {
                return value;
            }

            var type = typeof(T);

            var list = GetSensitiveFields(string.Empty, type);
            cache.Add(typeof(T), list);
            return list;
        }

       
    }

    private static List<string> GetSensitiveFields(string root, Type type)
    {
        var namingPolicy = JsonNamingPolicy.CamelCase;
        var list = new List<string>();
        foreach (var property in type.GetProperties())
        {
            string currentPath;
            currentPath = string.IsNullOrEmpty(root) ? $"{namingPolicy.ConvertName(property.Name)}" : $"{namingPolicy.ConvertName(root)}.{namingPolicy.ConvertName(property.Name)}";

            if (property.CustomAttributes.Any(x => x.AttributeType == typeof(SensitiveDataAttribute)))
            {
                list.Add(currentPath);
            }
            else
            {
                Type elementType = null!;

                //is a complex type of list of complex type
                if (property.PropertyType.IsArray)
                {
                    elementType = property.PropertyType.GetElementType()!;
                }
                else if (property.PropertyType.IsGenericType)
                {
                    elementType = property.PropertyType.GetGenericArguments()[0];

                }
                else if (property.PropertyType.IsClass)
                {
                    elementType = property.PropertyType;

                }

                if (elementType != null && elementType.Namespace != "System")
                {
                    list.AddRange(GetSensitiveFields($"{currentPath}", elementType!));
                }
            }
        }

        return list;
    }
}