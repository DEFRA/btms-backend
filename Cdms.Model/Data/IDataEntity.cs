namespace Cdms.Model.Data;

public interface IDataEntity
{
    public string? Id { get; set; }

    public string _Etag { get; set; }

    public DateTime Created { get; set; }

    public DateTime Updated { get; set; }
}