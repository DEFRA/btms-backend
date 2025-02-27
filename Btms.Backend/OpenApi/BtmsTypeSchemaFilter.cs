using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Btms.Backend.OpenApi;

/// <summary>
/// Set the title for the type if it's from BTMS.
/// </summary>
public class BtmsTypeSchemaFilter : ISchemaFilter
{
    private readonly string _namespacePrefix;

    public BtmsTypeSchemaFilter()
    {
        var namespacePrefix = typeof(BtmsTypeSchemaFilter).Namespace?.Split('.')[0];

        _namespacePrefix = namespacePrefix ?? throw new InvalidOperationException("Namespace prefix is null");
    }

    public void Apply(OpenApiSchema schema, SchemaFilterContext context)
    {
        ArgumentNullException.ThrowIfNull(schema);
        ArgumentNullException.ThrowIfNull(context);

        if (context.Type.Namespace?.StartsWith(_namespacePrefix) == true)
            schema.Title = context.Type.Name;
    }
}