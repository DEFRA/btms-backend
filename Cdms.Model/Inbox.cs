using Cdms.Model.Data;

namespace Cdms.Model;

public class Inbox : IDataEntity
{
    public string Id { get; set; }
    public string _Etag { get; set; }

    public string Type { get; set; }

    public object Data { get; set; }

    public DateTime Ts { get; set; }
}