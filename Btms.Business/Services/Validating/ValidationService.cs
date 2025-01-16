using Btms.Backend.Data;
using Btms.Business.Extensions;
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
        // var movementsToUpdate = new List<Movement>();
        // var importNotificationsToUpdate = new List<ImportNotification>();
        var valid = true;
        
        // var movementsWithoutDocs = linkResult.Movements
        //     .Where(m => m.BtmsStatus.LinkStatus != LinkStatusEnum.Error && m.Items.Any(i => i.Documents?.Length == 0));

        var hasNotifications = linkResult.Notifications.Count != 0;
        
        await Task.WhenAll(linkResult.Movements.Select(async m =>
        {
            var save = false;
            var errored = m.BtmsStatus.LinkStatus == LinkStatusEnum.Error;
            var chedTypes = m.BtmsStatus.ChedTypes;
            
            m.BtmsStatus = MovementStatus.Default();
            m.BtmsStatus.ChedTypes = chedTypes;
            
            if (!errored && m.Items.Any(i => i.Documents?.Length == 0))
            {
                m.BtmsStatus.LinkStatus = LinkStatusEnum.Error;
                m.BtmsStatus.LinkStatusDescription = "ALVSVAL318";
                m.BtmsStatus.Segment = MovementSegmentEnum.Cdms249;

                valid = false;
                save = true;
            }
            else if (hasNotifications && 
                     m.UniqueDocumentReferences().Length == 1 && 
                     m.Items.Any(i => i.Checks?.Any(d => d.CheckCode == "H219") ?? false))
            {
                m.BtmsStatus.Segment = linkResult.Notifications.Single().Status switch
                {
                    ImportNotificationStatusEnum.Validated => MovementSegmentEnum.Cdms205Ac1,
                    ImportNotificationStatusEnum.Rejected => MovementSegmentEnum.Cdms205Ac2,
                    ImportNotificationStatusEnum.PartiallyRejected => MovementSegmentEnum.Cdms205Ac3,
                    ImportNotificationStatusEnum.InProgress or ImportNotificationStatusEnum.Submitted => MovementSegmentEnum.Cdms205Ac4,
                    _ => null
                };

                save = true;
            }
            else if (hasNotifications && 
                     m.UniqueDocumentReferences().Length > 1 &&
                     m.Items
                         .SelectMany(i => (i.Checks ?? []).Select(d => d.CheckCode!))
                         .Count(d => d == "H219") > 1)
            {
                m.BtmsStatus.Segment = MovementSegmentEnum.Cdms205Ac5;

                save = true;
            }
            

            if (save)
            {
                await dbContext.Movements.Update(m, cancellationToken: cancellationToken);
            }
        }));
        
        // if (movementsWithoutDocs.Any())
        // {
        //     foreach (var movementsWithOutDoc in movementsWithoutDocs)
        //     {
        //         valid = false;
        //         
        //         movementsWithOutDoc.BtmsStatus.LinkStatus = LinkStatusEnum.Error;
        //         movementsWithOutDoc.BtmsStatus.LinkStatusDescription = "ALVSVAL318";
        //         movementsWithOutDoc.BtmsStatus.Segment = MovementSegmentEnum.Cdms249;
        //         
        //         movementsToUpdate.AddIfNotPresent(movementsWithOutDoc);
        //     }
        // }
        
        // Segment the movements for later use

        // if (linkResult.Notifications.Any())
        // {
        //     linkResult.Movements.ForEach(m =>
        //     {
        //         if (m.UniqueDocumentReferences().Length == 1 && 
        //             linkResult.Notifications.Single().Status == ImportNotificationStatusEnum.Validated &&
        //             m.Items.Any(i => i.Checks?.Any(d => d.CheckCode == "H219") ?? false)
        //         )
        //         {
        //             m.BtmsStatus.Segment = MovementSegmentEnum.Cdms205Ac1;
        //         
        //             movementsToUpdate.AddIfNotPresent(m);
        //         }
        //     });    
        // }
        
        // await importNotificationsToUpdate.ForEachAsync(async n => await dbContext.Notifications.Update(n, cancellationToken: cancellationToken));

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