
using Btms.Common.Extensions;
using Btms.Model.Relationships;

// ReSharper disable once CheckNamespace
namespace Btms.Types.Gvms.Mapping;

public static class GrmWithTransformMapper
{
    public static Btms.Model.Gvms.Gmr MapWithTransform(Gmr? from)
    {
        if (from is null)
        {
            return default!;
        }

        var gmr = GmrMapper.Map(from);
        Map(from, gmr);
        return gmr;
    }

    private static void Map(Gmr from, Btms.Model.Gvms.Gmr to)
    {
        to.CreatedSource = from.UpdatedSource;
    }
}