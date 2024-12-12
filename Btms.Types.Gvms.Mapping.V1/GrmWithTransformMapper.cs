using Btms.Model.Extensions;
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
        if (from.Declarations?.Customs is not null)
        {
            to.Relationships.Customs = new TdmRelationshipObject
            {
                Links = new RelationshipLinks
                {
                    Self = LinksBuilder.Gmr.BuildSelfRelationshipCustomsLink(":id"),
                    Related = LinksBuilder.Gmr.BuildRelatedCustomsLink(":id"),
                },
                Data = from.Declarations.Customs.Select(x => new RelationshipDataItem
                {
                    Id = x.Id!,
                    Type = "import-notifications"
                }).ToList()
            };
        }

        if (from.Declarations?.Transits is not null)
        {
            to.Relationships.Transits = new TdmRelationshipObject
            {
                Links = new RelationshipLinks
                {
                    Self = LinksBuilder.Gmr.BuildSelfRelationshipTransitsLink(":id"),
                    Related = LinksBuilder.Gmr.BuildRelatedTransitLink(":id"),
                },
                Data = from.Declarations.Transits.Select(x => new RelationshipDataItem
                {
                    Id = x.Id!,
                    Type = "movement"
                }).ToList()
            };
        }
    }
}