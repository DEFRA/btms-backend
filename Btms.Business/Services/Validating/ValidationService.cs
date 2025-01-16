using Btms.Backend.Data;
using Btms.Business.Services.Decisions;
using Btms.Business.Services.Linking;
using Btms.Common.Extensions;
using Btms.Metrics;
using Btms.Model;
using Btms.Model.Cds;
using Btms.Model.Extensions;
using Btms.Model.Ipaffs;
using Microsoft.Extensions.Logging;

namespace Btms.Business.Services.Validating;

public class ValidationService(IMongoDbContext dbContext, ValidationMetrics metrics, ILogger<ValidationService> logger) : IValidationService
{
    /// <summary>
    /// Before we attempt to match & make a decision we want to validate the state
    /// </summary>
    /// <param name="linkContext"></param>
    /// <param name="linkResult"></param>
    /// <param name="cancellationToken"></param>
    public async Task<bool> PostLinking(LinkContext linkContext, LinkResult linkResult, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("PreMatching");
        var movementsToUpdate = new List<Movement>();
        var importNotificationsToUpdate = new List<ImportNotification>();
        var valid = true;
        
        var movementsWithOutDocs = linkResult.Movements
            .Where(m => m.BtmsStatus.LinkStatus != LinkStatusEnum.Error && m.Items.Any(i => i.Documents?.Length == 0));
        
        if (movementsWithOutDocs.Any())
        {
            foreach (var movementsWithOutDoc in movementsWithOutDocs)
            {
                valid = false;
                
                movementsWithOutDoc.BtmsStatus.LinkStatus = LinkStatusEnum.Error;
                movementsWithOutDoc.BtmsStatus.LinkStatusDescription = "ALVSVAL318";
                movementsWithOutDoc.BtmsStatus.Status = MovementStatusEnum.Error;
                
                movementsToUpdate.AddIfNotPresent(movementsWithOutDoc);
            }
        }
        
        // Segment the movements for later use

        if (linkResult.Notifications.Any())
        {
            linkResult.Movements.ForEach(m =>
            {
                if (m.Items.Any(i => i.Checks?.Any(d => d.CheckCode == "H219") ?? false))
                {
                    m.BtmsStatus.Segment = MovementSegmentEnum.Cdms205Ac1;
                
                    movementsToUpdate.AddIfNotPresent(m);
                }
            });    
        }
        
        await movementsToUpdate.ForEachAsync(async m => await dbContext.Movements.Update(m, m._Etag, cancellationToken: cancellationToken));
        await importNotificationsToUpdate.ForEachAsync(async n => await dbContext.Notifications.Update(n, n._Etag, cancellationToken: cancellationToken));

        metrics.Validated();

        return valid;
    }
    
    public async Task<bool> PostDecision(LinkResult linkResult, DecisionResult decision, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("PostDecision");
        
        metrics.Validated();
        
        return await Task.FromResult(true);
    }
}