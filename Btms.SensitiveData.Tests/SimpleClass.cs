namespace Btms.SensitiveData.Tests;

public class SimpleClass
{
    [SensitiveData] public string SimpleStringOne { get; set; } = null!;

    public string SimpleStringTwo { get; set; } = null!;

    [SensitiveData] public string[] SimpleStringArrayOne { get; set; } = null!;

    public string[] SimpleStringArrayTwo { get; set; } = null!;


    public SimpleInnerClass[] SimpleObjectArray { get; set; } = null!;
}


public class SimpleInnerClass
{
    [SensitiveData] public string SimpleStringOne { get; set; } = null!;
}