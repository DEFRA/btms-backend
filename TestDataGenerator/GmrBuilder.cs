using System.Text.Json;
using Btms.Types.Gvms;

namespace TestDataGenerator;

public class GmrBuilder : GmrBuilder<Gmr>
{
    private GmrBuilder()
    {
    }
    
    public GmrBuilder(string file) : base(file)
    {
    }
    
    public static GmrBuilder Default()
    {
        return new GmrBuilder();
    }
}

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