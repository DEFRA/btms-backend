using System.ComponentModel.DataAnnotations;

namespace Cdms.Backend.Data;

public class MongoDbOptions
{
    public const string SectionName = "Mongo";

    [Required] public string DatabaseUri { get; set; }

    [Required] public string DatabaseName { get; set; }
}