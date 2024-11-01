using System.Reflection;

namespace Cdms.Common;

public static class ReflectionUtils
{
    public static TResult GetPrivateFieldValue<TResult>(this object instance, string fieldName)
    {
        var field = instance.GetType()
            .GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);

        return (TResult)field.GetValue(instance);
    }
}