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
using SlimMessageBus.Host;

namespace Btms.Business.Services.Validating;

public class ValidationService(IMongoDbContext dbContext, ValidationMetrics metrics, ILogger<ValidationService> logger) : IValidationService
{
    /// <summary>
    /// Before we attempt to match & make a decision we want to validate the state
    /// </summary>
    /// <param name="linkContext"></param>
    /// <param name="linkResult"></param>
    /// <param name="cancellationToken"></param>
    public async Task<bool> PostLinking(LinkContext linkContext, LinkResult linkResult, 
        Movement? triggeringMovement = null, ImportNotification? triggeringNotification = null,
        CancellationToken cancellationToken = default)
    {
        if (!(triggeringMovement.HasValue() || triggeringNotification.HasValue()))
        {
            throw new InvalidOperationException(
                "Can't call PostLinking without either a movement or notification.");
        }
        
        logger.LogInformation("PreMatching");
        
        var valid = true;
        
        var hasNotifications = linkResult.Notifications.Count != 0;
        
        await Task.WhenAll(linkResult.Movements.Select(async m =>
        {
            if (m.BtmsStatus.LinkStatus == LinkStatusEnum.Error) return;
            
            var chedTypes = m.BtmsStatus.ChedTypes;
            
            var notificationRelationshipIds = m.UniqueNotificationRelationshipIds();
            var documentReferenceIds = m.UniqueDocumentReferenceIdsThatShouldLink();

            m.BtmsStatus = MovementStatus.Default();
            m.BtmsStatus.ChedTypes = chedTypes;
            m.BtmsStatus.LinkStatus = documentReferenceIds.Count == 0
                ? LinkStatusEnum.NoLinks
                : notificationRelationshipIds.Count() == documentReferenceIds.Count() &&
                  notificationRelationshipIds.All(documentReferenceIds.Contains)
                    ? LinkStatusEnum.AllLinked
                    : notificationRelationshipIds.Count == 0 && documentReferenceIds.Count != 0
                        ? LinkStatusEnum.MissingLinks
                        : notificationRelationshipIds.Count < documentReferenceIds.Count()
                            ? LinkStatusEnum.PartiallyLinked
                            : LinkStatusEnum.Investigate;
                
            if (m.Items.Any(i => i.Documents?.Length == 0))
            {
                // One of the error states from CDMS-242
                // https://eaflood.atlassian.net/wiki/spaces/ALVS/pages/5400723501/To-be+HMRCErrorNotification+-+Error+Codes
                
                m.BtmsStatus.Status = MovementStatusEnum.FeatureMissing;
                m.BtmsStatus.LinkStatusDescription = "ALVSVAL318";
                m.BtmsStatus.Segment = MovementSegmentEnum.Cdms249;
                
                valid = false;
            }
            else if (hasNotifications && 
                     documentReferenceIds.Count == 1 && 
                     m.Items.Any(i => i.Checks?.Any(d => d.CheckCode == "H219") ?? false))
            {
                m.BtmsStatus.Status = MovementStatusEnum.FeatureMissing;
                m.BtmsStatus.Segment = linkResult.Notifications.Single().Status switch
                {
                    ImportNotificationStatusEnum.Validated => MovementSegmentEnum.Cdms205Ac1,
                    ImportNotificationStatusEnum.Rejected => MovementSegmentEnum.Cdms205Ac2,
                    ImportNotificationStatusEnum.PartiallyRejected => MovementSegmentEnum.Cdms205Ac3,
                    ImportNotificationStatusEnum.InProgress or ImportNotificationStatusEnum.Submitted => MovementSegmentEnum.Cdms205Ac4,
                    _ => null
                };
            }
            else if (hasNotifications && 
                     m.UniqueDocumentReferences().Length > 1 &&
                     m.Items
                         .SelectMany(i => (i.Checks ?? []).Select(d => d.CheckCode!))
                         .Count(d => d == "H219") > 1)
            {
                m.BtmsStatus.Status = MovementStatusEnum.FeatureMissing;
                m.BtmsStatus.Segment = MovementSegmentEnum.Cdms205Ac5;
            }

            await dbContext.Movements.Update(m, cancellationToken: cancellationToken);
            
        }));
        
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