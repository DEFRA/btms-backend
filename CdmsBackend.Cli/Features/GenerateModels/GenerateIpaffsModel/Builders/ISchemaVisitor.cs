namespace CdmsBackend.Cli.Features.GenerateModels.GenerateIpaffsModel.Builders;

public interface ISchemaVisitor
{
    void OnProperty(PropertyVisitorContext context);

    void OnDefinition(DefinitionVisitorContext context);
}