using System.Reflection;
using System.Text;

namespace Cdms.Common
{
    public static class ReflectionUtils
    {
        public static TResult GetPrivateFieldValue<TResult>(this object instance, string fieldName)
        {
            var field = instance.GetType()
                .GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);

            return (TResult)field.GetValue(instance);
        }
    }

    public static class ObservabilityUtils
    {
        public static string FormatTypeName(StringBuilder sb, Type type)
        {
            if (type.IsGenericParameter)
                return "";

            if (type.IsGenericType)
            {
                var name = type.GetGenericTypeDefinition().Name;

                //remove `1
                var index = name.IndexOf('`');
                if (index > 0)
                    name = name.Remove(index);

                sb.Append(name);
                sb.Append('_');
                Type[] arguments = type.GenericTypeArguments;
                for (var i = 0; i < arguments.Length; i++)
                {
                    if (i > 0)
                        sb.Append('_');

                    FormatTypeName(sb, arguments[i]);
                }
            }
            else
                sb.Append(type.Name);

            return sb.ToString();
        }
    }
}