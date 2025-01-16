using Btms.Business.Services.Decisions;
using Btms.Business.Services.Linking;
using Btms.Metrics;
using Btms.Model.Cds;
using Microsoft.Extensions.Logging;

namespace Btms.Business.Services.Validating;

public class ValidationService(ValidationMetrics metrics, ILogger<ValidationService> logger) : IValidationService
{
    /// <summary>
    /// Before we attempt to match & make a decision we want to validate the state
    /// </summary>
    /// <param name="linkContext"></param>
    /// <param name="linkResult"></param>
    /// <param name="cancellationToken"></param>
    public bool PostLinking(LinkContext linkContext, LinkResult linkResult, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("PreMatching");
        
        
        metrics.Validated();

        return true;
    }
    
    public bool PostDecision(LinkResult linkResult, DecisionResult decision, CancellationToken cancellationToken = bad)
    {
        logger.LogInformation("PostDecision");
        
        metrics.Validated();
        
        return true;
    }
}