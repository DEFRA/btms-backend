using SlimMessageBus;
using System.Diagnostics;
using Amazon.SQS.Model;
using Azure.Messaging.ServiceBus;
using Btms.Backend.Data;
using Btms.Business.Builders;
using Btms.Common.Extensions;
using Btms.Model;
using Btms.Model.Cds;
using Btms.Model.Extensions;
using Btms.Model.Ipaffs;
using Microsoft.EntityFrameworkCore;

namespace Btms.Consumers.Extensions;

public static class MongoDbContextExtensions
{
    public static List<DecisionImportNotifications> GetDecisionImportNotifications(this IMongoCollectionSet<ImportNotification> dbSet, IEnumerable<string> importNotifications)
    {
        return dbSet
            .Where(n => importNotifications.Contains(n.Id))
            .Select(n => new DecisionImportNotifications()
            {
                Id = n.Id!,
                Version = n.Version,
                Created = n.Created,
                Updated = n.Updated,
                UpdatedEntity = n.UpdatedEntity,
                CreatedSource = n.CreatedSource!.Value,
                UpdatedSource = n.UpdatedSource!.Value,
                AutoClearedOn = n.PartTwo == null ? n.PartTwo!.AutoClearedOn : null,
                Status = n.Status,
                Type = n.ImportNotificationType!.Value
            })
            .ToList();
    }

    public static void UpdateAlvsDecisionStatusImportNotificationState(this IMongoCollectionSet<Movement> dbSet, List<Movement> movements, ImportNotification notification)
    {
        var decisionImportNotification = notification.AsDecisionImportNotification();
        movements.ForEach(m =>
        {
            if (m.AlvsDecisionStatus
                .Context.ImportNotifications.HasValue())
            {
                m.AlvsDecisionStatus
                    .Context.ImportNotifications!.Replace(n =>
                            n.Id == notification.Id,
                        decisionImportNotification);

                m.Status.BusinessDecisionStatus = m.GetBusinessDecisionStatus();
                m.Status.NonComparableDecisionReason = m.GetDecisionCategory();
            }
            else
            {
                m.AlvsDecisionStatus
                        .Context.ImportNotifications =
                    new List<DecisionImportNotifications>() { decisionImportNotification };
            }

            dbSet.Update(m);
        });
    }
}