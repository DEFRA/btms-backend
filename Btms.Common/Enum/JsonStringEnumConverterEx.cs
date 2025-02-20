using System.Runtime.Serialization;
using System.Text.Json;
using System.Text.Json.Serialization;
using Btms.Common.Extensions;

namespace Btms.Common.Enum;

public class JsonStringEnumConverterEx<TEnum> : JsonConverter<TEnum> where TEnum : struct, System.Enum
{
    private readonly Dictionary<TEnum, string> _enumToString = new();
    private readonly Dictionary<string, TEnum> _stringToEnum = new();
    private readonly Dictionary<int, TEnum> _numberToEnum = new();

    public JsonStringEnumConverterEx()
    {
        var type = typeof(TEnum);

        foreach (var value in System.Enum.GetValues<TEnum>())
        {
            var enumMember = type.GetMember(value.ToString())[0];
            var attr = enumMember.GetCustomAttributes(typeof(EnumMemberAttribute), false)
                .Cast<EnumMemberAttribute>()
                .FirstOrDefault();

            _stringToEnum.Add(value.ToString(), value);
            var num = Convert.ToInt32(type.GetField("value__")!
                .GetValue(value));
            if (attr?.Value != null)
            {
                _enumToString.Add(value, attr.Value);
                _numberToEnum.Add(num, value);

                if (attr.Value != value.ToString())
                {
                    //Don't attempt to add the same value again if the EnumMember Value is the
                    //same as the enum value
                    _stringToEnum.Add(attr.Value, value);
                }
            }
            else
            {
                _enumToString.Add(value, value.ToString());
            }
        }
    }

    public override TEnum Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var type = reader.TokenType;
        if (type == JsonTokenType.String)
        {
            var stringValue = reader.GetString()!;
            if (_stringToEnum.TryGetValue(stringValue, out var enumValue))
            {
                return enumValue;
            }
        }
        else if (type == JsonTokenType.Number)
        {
            var numValue = reader.GetInt32();
            _numberToEnum.TryGetValue(numValue, out var enumValue);
            return enumValue;
        }

        return default;
    }

    public override void Write(Utf8JsonWriter writer, TEnum value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(_enumToString[value]);
    }

    public string[] GetValues()
    {
        return _enumToString.Values.ToArray();
    }

    public string GetValue(TEnum value)
    {
        return _enumToString[value];
    }
}