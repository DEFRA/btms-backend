using System.Xml.Schema;

namespace CdmsBackend.Cli.Features.GenerateModels.GenerateAlvsModel.Commands;

public static class XmlSchemaExtensions
{
    public static string GetDescription(this XmlSchemaComplexType complexType)
    {
        if (complexType.Annotation is not null)
        {
            foreach (var annotationItem in complexType.Annotation?.Items!)
            {
                if (annotationItem is XmlSchemaDocumentation documentation)
                {
                    return documentation.Markup?.FirstOrDefault()?.Value!;
                }
            }
        }

        return string.Empty;
    }


    public static string GetSchemaType(this XmlSchemaElement schemaElement)
    {

        if (schemaElement.SchemaType is not null)
        {
            if (schemaElement.SchemaType is XmlSchemaSimpleType { Content: XmlSchemaSimpleTypeRestriction simpleTypeRestriction })
            {
                return simpleTypeRestriction.BaseTypeName.Name == "integer" ? "int" : simpleTypeRestriction.BaseTypeName.Name;
            }
            else if (schemaElement.SchemaType is XmlSchemaComplexType)
            {
                return schemaElement.Name!;
            }
        }

        if (schemaElement.SchemaTypeName.Name == "int")
        {
            return schemaElement.SchemaTypeName.Name;
        }

        if (schemaElement.SchemaTypeName.Name == "dateTime" )
        {
            return "DateTime";
        }

        var schema = schemaElement.Parent;
        while (schema?.Parent is not null)
        {
            schema = schema.Parent;
        }
        //schemaElement.SchemaTypeName

        foreach (var item in ((XmlSchema)schema!)?.Items!)
        {
            if (item is XmlSchemaType schemaType && schemaType.Name == schemaElement.SchemaTypeName.Name)
            {
                if (schemaType is XmlSchemaSimpleType { Content: XmlSchemaSimpleTypeRestriction simpleTypeRestriction })
                {
                    return simpleTypeRestriction.BaseTypeName.Name;
                }
                else if (schemaType is XmlSchemaComplexType complexType)
                {
                    return complexType.Name!;
                }


            }
        }

        return string.Empty;
    }
}