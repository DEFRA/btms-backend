
namespace Btms.Business.Services.Linking;

public interface ILinkingService
{
    Task<LinkResult> Link(LinkContext linkContext, CancellationToken cancellationToken = default);
}