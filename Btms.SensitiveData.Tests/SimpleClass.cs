using System.Text.Json.Serialization;

namespace Btms.SensitiveData.Tests;

public class SimpleClass
{
    [JsonPropertyName("simpleStringOne")]
    public string SimpleStringOne { get; set; } = null!;

    [JsonPropertyName("simpleStringTwo")]
    public string SimpleStringTwo { get; set; } = null!;

    [JsonPropertyName("simpleStringArrayOne")]
    public string[] SimpleStringArrayOne { get; set; } = null!;

    [JsonPropertyName("simpleStringArrayTwo")]
    public string[] SimpleStringArrayTwo { get; set; } = null!;

    [JsonPropertyName("simpleObjectArray")]
    public SimpleInnerClass[] SimpleObjectArray { get; set; } = null!;
}


public class SimpleInnerClass
{
    [JsonPropertyName("simpleStringOne")]
    public string SimpleStringOne { get; set; } = null!;
}

public class SimpleClassSensitiveFieldsProvider : ISensitiveFieldsProvider
{
    public List<string> Get(Type type)
    {
        return ["simpleStringOne", "simpleStringArrayOne", "simpleObjectArray.simpleStringOne"];
    }
}