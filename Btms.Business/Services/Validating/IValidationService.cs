using Btms.Business.Services.Decisions;
using Btms.Business.Services.Linking;
using Btms.Model;
using Btms.Model.Cds;
using Btms.Model.Ipaffs;

namespace Btms.Business.Services.Validating;

public interface IValidationService
{
    /// <summary>
    /// Before we attempt to match & make a decision we want to validate the state
    /// </summary>
    /// <param name="linkContext"></param>
    /// <param name="triggeringNotification"></param>
    /// <param name="cancellationToken"></param>
    /// <param name="linkResult"></param>
    /// <param name="triggeringMovement"></param>
    Task<bool> PostLinking(LinkContext linkContext, LinkResult linkResult,
        Movement? triggeringMovement = null, ImportNotification? triggeringNotification = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="linkResult"></param>
    /// <param name="decision"></param>
    /// <param name="cancellationToken"></param>
    Task<bool> PostDecision(LinkResult linkResult, DecisionResult decision, CancellationToken cancellationToken = default);
}