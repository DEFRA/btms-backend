using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

namespace Btms.Model.ChangeLog;

public class ChangeSetTypeInfoResolver : DefaultJsonTypeInfoResolver
{
    public override JsonTypeInfo GetTypeInfo(Type type, JsonSerializerOptions options)
    {
        var typeInfo = base.GetTypeInfo(type, options);

        if (typeInfo.Kind == JsonTypeInfoKind.Object)
        {
            foreach (var property in typeInfo.Properties)
            {
                if (property.AttributeProvider!.GetCustomAttributes(typeof(ChangeSetIgnoreAttribute), false).Any())
                {
                    property.ShouldSerialize = (_, _) => false;
                }
            }
        }

        return typeInfo;
    }
}