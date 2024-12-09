using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

namespace Btms.Model.ChangeLog;

public class ChangeSetTypeInfoResolver : DefaultJsonTypeInfoResolver
{
    public override JsonTypeInfo GetTypeInfo(Type type, JsonSerializerOptions options)
    {
        JsonTypeInfo typeInfo = base.GetTypeInfo(type, options);

        if (typeInfo.Kind == JsonTypeInfoKind.Object)
        {
            foreach (var property in typeInfo.Properties)
            {
                if (property.AttributeProvider!.GetCustomAttributes(typeof(ChangeSetIgnoreAttribute), false).Any())
                {
                    property.ShouldSerialize = (o, o1) => false;
                }
            }
        }

        return typeInfo;
    }
}