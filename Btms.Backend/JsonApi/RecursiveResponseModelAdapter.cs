using System.Diagnostics.CodeAnalysis;
using Btms.Model.Relationships;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Middleware;
using JsonApiDotNetCore.Queries;
using JsonApiDotNetCore.QueryStrings;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Serialization.Objects;
using JsonApiDotNetCore.Serialization.Response;
using MongoDB.Driver;
using RelationshipLinks = JsonApiDotNetCore.Serialization.Objects.RelationshipLinks;

namespace Btms.Backend.JsonApi;

[SuppressMessage("SonarLint", "S107",
    Justification =
        "This is to override a class which is part of JsonApi so this requires the 8 parameters")]
public class RecursiveResponseModelAdapter : ResponseModelAdapter
{
    private readonly IRequestQueryStringAccessor _requestQueryStringAccessor;
    
    public RecursiveResponseModelAdapter(
        IJsonApiRequest request,
        IJsonApiOptions options,
        ILinkBuilder linkBuilder,
        IMetaBuilder metaBuilder,
        IResourceDefinitionAccessor resourceDefinitionAccessor,
        IEvaluatedIncludeCache evaluatedIncludeCache,
        ISparseFieldSetCache sparseFieldSetCache,
        IRequestQueryStringAccessor requestQueryStringAccessor) : base(request, options, linkBuilder, metaBuilder, resourceDefinitionAccessor, evaluatedIncludeCache, sparseFieldSetCache, requestQueryStringAccessor)
    {
        _requestQueryStringAccessor = requestQueryStringAccessor;
    }

    
    // private readonly IResponseModelAdapter inner = new ResponseModelAdapter(request, options, linkBuilder, metaBuilder,
    //     resourceDefinitionAccessor,
    //     evaluatedIncludeCache, sparseFieldSetCache, requestQueryStringAccessor);

    public new Document Convert(object? model)
    {
        // var document = inner.Convert(model);
        // if (document.Data.Value is null)
        // {
        //     return document;
        // }
        //
        // var listOfResourceObjects = document.Data.ManyValue is not null
        //     ? document.Data.ManyValue.ToList()
        //     : [document.Data.SingleValue!];
        //
        // foreach (var resourceObject in listOfResourceObjects)
        // {
        //     if (resourceObject.Attributes!.TryGetValue("relationships", out var value))
        //     {
        //         ProcessRelationships(value, resourceObject);
        //     }
        // }
        //
        // return document;

        throw new NotImplementedException();
    }
//
//     private static void ProcessRelationships(object? value, ResourceObject resourceObject)
//     {
//         var relationships = (value as ITdmRelationships)?.GetRelationshipObjects();
//         resourceObject.Relationships = new Dictionary<string, RelationshipObject?>();
//
//         foreach (var relationship in relationships!)
//         {
// #pragma warning disable CS8602 // Dereference of a possibly null reference.
//             List<ResourceIdentifierObject> list = relationship.Item2.Data.Select(item =>
//                     new ResourceIdentifierObject
//                     {
//                         Type = item.Type,
//                         Id = item.Id,
//                         Meta = item.ToDictionary(),
//                     })
//                 .ToList();
// #pragma warning restore CS8602 // Dereference of a possibly null reference.
//
//
//             var meta = new Dictionary<string, object?>();
//             resourceObject.Relationships.Add(relationship.Item1,
//                 new RelationshipObject
//                 {
//                     Meta = meta,
//                     Links = new RelationshipLinks
//                     {
//                         Self = relationship.Item2.Links?.Self,
//                         Related = relationship.Item2.Links?.Related
//                     },
//                     Data = new SingleOrManyData<ResourceIdentifierObject>(list)
//                 });
//         }
//
//         resourceObject.Attributes!.Remove("relationships");
//     }
}