using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Btms.Backend.OpenApi;

public class DocumentFilter : IDocumentFilter
{
    public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
    {
        context.SchemaGenerator.GenerateSchema(
            typeof(NotificationResourceResponse),
            context.SchemaRepository
        );

        swaggerDoc.AddPath(
            path: "import-notifications",
            pathDescription: "Notification Operations",
            operationDescription: "Get Notifications",
            referenceId: typeof(NotificationResourceResponse).FullName!,
            tag: "Notifications"
        );

        context.SchemaGenerator.GenerateSchema(
            typeof(MovementResourceResponse),
            context.SchemaRepository
        );

        swaggerDoc.AddPath(
            path: "movements",
            pathDescription: "Movement Operations",
            operationDescription: "Get Movements",
            referenceId: typeof(MovementResourceResponse).FullName!,
            tag: "Movements"
        );

        context.SchemaGenerator.GenerateSchema(
            typeof(GoodsMovementResourceResponse),
            context.SchemaRepository
        );

        swaggerDoc.AddPath(
            path: "gmrs",
            pathDescription: "Goods Movement Operations",
            operationDescription: "Get Goods Movements",
            referenceId: typeof(GoodsMovementResourceResponse).FullName!,
            tag: "GoodsMovements"
        );
    }
}