using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Btms.Backend.OpenApi;

public class DocumentFilter : IDocumentFilter
{
    public DocumentFilter()
    {
        Thread.Sleep(20);
    }

    public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
    {
        context.SchemaGenerator.GenerateSchema(typeof(NotificationResourceResponse), context.SchemaRepository);
        
        swaggerDoc.AddPath(
            path: "import-notifications",
            pathDescription: "Notification Operations",
            operationDescription: "Get Notifications",
            referenceId: "NotificationResourceResponse",
            tag: "Notifications");
        
        context.SchemaGenerator.GenerateSchema(typeof(MovementResourceResponse), context.SchemaRepository);
        
        swaggerDoc.AddPath(
            path: "movements",
            pathDescription: "Movement Operations",
            operationDescription: "Get Movements",
            referenceId: "MovementResourceResponse",
            tag: "Movements");
    }
}