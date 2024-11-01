using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace Cdms.Common;

public static class ReflectionUtils
{
    [SuppressMessage("SonarLint", "S3011",
        Justification =
            "Ignored as this is required to get the internal queue counts from slim message bus, until its exposed")]
    public static TResult GetPrivateFieldValue<TResult>(this object instance, string fieldName)
    {
        var field = instance.GetType()
            .GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);

        return (TResult)field.GetValue(instance);
    }
}