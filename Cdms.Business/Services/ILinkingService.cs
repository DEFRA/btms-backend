
namespace Cdms.Business.Services;

public interface ILinkingService
{
    Task<LinkResult> Link(LinkContext linkContext, CancellationToken cancellationToken = default);
}