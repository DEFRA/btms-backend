using Btms.Business.Services.Decisions;
using Btms.Business.Services.Linking;
using Btms.Model.Cds;

namespace Btms.Business.Services.Validating;

public interface IValidationService
{
    /// <summary>
    /// Before we attempt to match & make a decision we want to validate the state
    /// </summary>
    /// <param name="linkContext"></param>
    /// <param name="cancellationToken"></param>
    Task<bool> PostLinking(LinkContext linkContext, LinkResult linkResult, CancellationToken cancellationToken = default);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="linkResult"></param>
    /// <param name="decision"></param>
    /// <param name="cancellationToken"></param>
    Task<bool> PostDecision(LinkResult linkResult, DecisionResult decision, CancellationToken cancellationToken = default);
}