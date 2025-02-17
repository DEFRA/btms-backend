using System.Reflection;
using Btms.Backend.OpenApi;
using Microsoft.OpenApi.Models;

namespace Btms.Backend.Swagger;

public static class SwaggerGen
{
    public static bool SwaggerGenEntrypoint(string[] args)
    {
        if (Assembly.GetEntryAssembly()?.GetName().Name != "dotnet-swagger") return false;

        var builder = WebApplication.CreateBuilder(args);
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("public-v0.1", new OpenApiInfo { Title = "BTMS Public API", Version = "v0.1" });
            c.DocumentFilter<DocumentFilter>();
            c.SchemaFilter<SchemaFilter>();
            c.UseAllOfToExtendReferenceSchemas();
            c.ResolveConflictingActions(apiDescriptions => apiDescriptions.First());
        });

        var appForGen = builder.Build();
        appForGen.UseSwagger();
        appForGen.UseSwaggerUI(options =>
        {
            options.SwaggerEndpoint("/swagger/public-v0.1/swagger.json", "public");
        });

        appForGen.Run();
        return true;
    }
}