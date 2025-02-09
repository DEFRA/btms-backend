using System.Text.Json;
using Btms.Types.Gvms;

namespace TestDataGenerator;

public class GmrBuilder(string file) : GmrBuilder<Gmr>(file);

public class GmrBuilder<T> : BuilderBase<T, GmrBuilder<T>> where T : Gmr
{
    protected GmrBuilder(string? file = null, string? itemJson = null)
        : base(file, itemJson, new JsonSerializerOptions(JsonSerializerDefaults.Web))
    {
    }

    protected override GmrBuilder<T> Validate()
    {
        return this;
    }
}