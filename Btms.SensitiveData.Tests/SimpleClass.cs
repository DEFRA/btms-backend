using Btms.SensitiveData;

namespace Btms.SensitiveData.Tests
{
    public class SimpleClass
    {
        [SensitiveData] public string SimpleStringOne { get; set; } = null!;

        public string SimpleStringTwo { get; set; } = null!;

        [SensitiveData] public string[] SimpleStringArrayOne { get; set; } = null!;

        public string[] SimpleStringArrayTwo { get; set; } = null!;
    }
}