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

public class SearchGmrsBuilder : SearchGmrsBuilder<SearchGmrsForDeclarationIdsResponse>
{
    private SearchGmrsBuilder()
    {
    }

    public SearchGmrsBuilder(string file) : base(file)
    {
    }

    public static SearchGmrsBuilder Default()
    {
        return new SearchGmrsBuilder();
    }
}

public class SearchGmrsBuilder<T> : BuilderBase<T, SearchGmrsBuilder<T>> where T : SearchGmrsForDeclarationIdsResponse
{
    protected SearchGmrsBuilder(string? file = null, string? itemJson = null)
        : base(file, itemJson, new JsonSerializerOptions(JsonSerializerDefaults.Web))
    {
    }

    protected override SearchGmrsBuilder<T> Validate()
    {
        return this;
    }
}