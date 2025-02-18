using System.Reflection;

namespace Btms.SensitiveData;

public interface ISensitiveFieldsProvider
{
    List<string> Get(Type type);
}


public class SensitiveFieldsProvider : ISensitiveFieldsProvider
{
    private static Dictionary<Type, List<string>> _cache = new();
    private static readonly object CacheLock = new();

    public List<string> Get(Type type)
    {
        lock (CacheLock)
        {
            if (_cache.TryGetValue(type, out var value))
            {
                return value;
            }

            var list = GetSensitiveFields(type);
            _cache.Add(type, list);
            return list;
        }
    }

    private static List<string> GetSensitiveFields(Type type)
    {
        var list = new List<string>();

        var resourceName = $"Btms.SensitiveData.FieldsList.{type.Name}.txt";

        using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName)!)
        using (StreamReader reader = new StreamReader(stream))
        {
            while (reader.Peek() != -1)
            {
                var s = reader.ReadLine();
                if (s != null) list.Add(s);
            }
        }

        return list;
    }
}