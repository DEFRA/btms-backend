namespace Btms.Model;

public interface IResource
{
    public DateTime? CreatedSource { get; set; }

    public DateTime? UpdatedSource { get; set; }

    public DateTime Updated { get; set; }
}