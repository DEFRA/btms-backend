using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

namespace Btms.SensitiveData;

public class SensitiveDataTypeInfoResolver(SensitiveDataOptions sensitiveDataOptions) : DefaultJsonTypeInfoResolver
{
    public override JsonTypeInfo GetTypeInfo(Type type, JsonSerializerOptions options)
    {
        JsonTypeInfo typeInfo = base.GetTypeInfo(type, options);

        if (typeInfo.Kind == JsonTypeInfoKind.Object)
        {
            foreach (var property in typeInfo.Properties)
            {
                if (!sensitiveDataOptions.Include && property.AttributeProvider!.GetCustomAttributes(typeof(SensitiveDataAttribute), false).Any())
                {
                    if (property.PropertyType == typeof(string))
                    {
                        property.CustomConverter =
                            new SensitiveDataRedactedConverter(sensitiveDataOptions);
                    }
                    else if (property.PropertyType == typeof(string[]))
                    {
                        property.CustomConverter =
                            new StringArraySensitiveDataRedactedConverter(sensitiveDataOptions);
                    }
                }
            }
        }

        return typeInfo;
    }
}