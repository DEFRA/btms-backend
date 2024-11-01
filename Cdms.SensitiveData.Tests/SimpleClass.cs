namespace Cdms.SensitiveData.Tests
{
    public class SimpleClass
    {
        [SensitiveData] public string SimpleStringOne { get; set; }

        public string SimpleStringTwo { get; set; }

        [SensitiveData] public string[] SimpleStringArrayOne { get; set; }

        public string[] SimpleStringArrayTwo { get; set; }
    }
}