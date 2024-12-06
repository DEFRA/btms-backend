using Btms.Backend.Data;
using Btms.Backend.Data.Extensions;
using Btms.Metrics;
using Btms.Model;
using Btms.Model.Ipaffs;
using Btms.Model.Relationships;
using Microsoft.Extensions.Logging;

namespace Btms.Business.Services;

public static partial class LinkingServiceLogging
{
    [LoggerMessage(Level = LogLevel.Information, Message = "Linking Started for {ContextType} - {MatchIdentifier}")]
    internal static partial void LinkingStarted(this ILogger logger, string contextType, string matchIdentifier);

    [LoggerMessage(Level = LogLevel.Information, Message = "Linking Finished for {ContextType} - {MatchIdentifier}")]
    internal static partial void LinkingFinished(this ILogger logger, string contextType, string matchIdentifier);

    [LoggerMessage(Level = LogLevel.Error, Message = "Linking Failed for {ContextType} - {MatchIdentifier}")]
    internal static partial void LinkingFailed(this ILogger logger, Exception exception, string contextType, string matchIdentifier);

    [LoggerMessage(Level = LogLevel.Information, Message = "Linking Not Found for {ContextType} - {MatchIdentifier}")]
    internal static partial void LinkNotFound(this ILogger logger, string contextType, string matchIdentifier);

    [LoggerMessage(Level = LogLevel.Information, Message = "Link Found for {ContextType} - {MatchIdentifier} - {MovementsCount} Movements and {NotificationsCount} Notifications")]
    internal static partial void LinkFound(this ILogger logger, string contextType, string matchIdentifier, int movementsCount, int notificationsCount);

     [LoggerMessage(Level = LogLevel.Information, Message = "Linking Not attempted for {ContextType} - {MatchIdentifier}")]
    internal static partial void LinkNotAttempted(this ILogger logger, string contextType, string matchIdentifier);
}

public class LinkingService(IMongoDbContext dbContext, LinkingMetrics metrics, ILogger<LinkingService> logger) : ILinkingService
{
    public async Task<LinkResult> Link(LinkContext linkContext, CancellationToken cancellationToken = default)
    {
        var startedAt = TimeProvider.System.GetTimestamp();
        LinkResult result;
        using (logger.BeginScope(new List<KeyValuePair<string, object>>
               {
                   new("MatchIdentifier", linkContext.GetIdentifiers()),
                   new("ContextType", linkContext.GetType().Name),
               }))
        {
            logger.LinkingStarted(linkContext.GetType().Name, linkContext.GetIdentifiers());
            try
            {
                switch (linkContext)
                {
                    case MovementLinkContext movementLinkContext:
                        if (!ShouldLink(movementLinkContext))
                        {
                            logger.LinkNotAttempted(linkContext.GetType().Name, linkContext.GetIdentifiers());
                            return new LinkResult(LinkState.NotLinked);
                        }

                        result = await FindMovementLinks(movementLinkContext.PersistedMovement, cancellationToken);
                        break;
                    case ImportNotificationLinkContext notificationLinkContext:
                        if (!ShouldLink(notificationLinkContext))
                        {
                            logger.LinkNotAttempted(linkContext.GetType().Name, linkContext.GetIdentifiers());
                            return new LinkResult(LinkState.NotLinked);
                        }

                        result = await FindImportNotificationLinks(notificationLinkContext.PersistedImportNotification,
                            cancellationToken);
                        break;
                    default: throw new ArgumentException("context type not supported");
                }


                if (result.State == LinkState.NotLinked)
                {
                    logger.LinkNotFound(linkContext.GetType().Name, linkContext.GetIdentifiers());
                    return result;
                }

                logger.LinkFound(linkContext.GetType().Name, linkContext.GetIdentifiers(), result.Movements.Count, result.Notifications.Count);

                metrics.Linked<Movement>(result.Movements.Count);
                metrics.Linked<ImportNotification>(result.Notifications.Count);

                using var transaction = await dbContext.StartTransaction(cancellationToken);
                foreach (var notification in result.Notifications)
                {
                    foreach (var movement in result.Movements)
                    {
                        notification.AddRelationship(new TdmRelationshipObject()
                        {
                            Links = RelationshipLinks.CreateForNotification(notification),
                            Data =
                            [
                                RelationshipDataItem.CreateFromMovement(notification, movement,
                                    notification._MatchReference)
                            ]
                        });

                        movement.AddRelationship(new TdmRelationshipObject()
                        {
                            Links = RelationshipLinks.CreateForMovement(movement),
                            Data =
                            [
                                RelationshipDataItem.CreateFromNotification(notification, movement,
                                    notification._MatchReference)
                            ]
                        });

                        await dbContext.Movements.Update(movement, movement._Etag, transaction, cancellationToken);
                        await dbContext.Notifications.Update(notification, notification._Etag, transaction,
                            cancellationToken);
                    }
                }

                await transaction.CommitTransaction(cancellationToken);
            }
            catch (Exception e)
            {
                metrics.Faulted(e);
                throw new LinkException(e);
            }
            finally
            {
                var e = TimeProvider.System.GetElapsedTime(startedAt);
                metrics.Completed(e.TotalMilliseconds);
                logger.LinkingFinished(linkContext.GetType().Name, linkContext.GetIdentifiers());
            }
        }

       

        return result;
    }

    private static bool ShouldLink(MovementLinkContext movContext)
    {
        if (movContext.ExistingMovement is null) return true;

        var existingItems = movContext.ExistingMovement.Items is null ? [] : movContext.ExistingMovement.Items;
        var receivedItems = movContext.PersistedMovement.Items is null ? [] : movContext.PersistedMovement.Items;

        // Diff movements for fields of interest
        var existingDocs = existingItems
            .SelectMany(x => x.Documents ?? [])
            .Select(d => d.DocumentReference
            ).ToList();

        var receivedDocs = receivedItems
            .SelectMany(x => x.Documents ?? [])
            .Select(d => d.DocumentReference).ToList();

        if (existingDocs.Count != receivedDocs.Count ||
            !existingDocs.TrueForAll(receivedDocs.Contains))
        {
            // Delta in received Docs
            return true;
        }

        return false;
    }

    private static bool ShouldLink(ImportNotificationLinkContext notifContext)
    {
        if (notifContext.ExistingImportNotification is null) return true;

        var existingCommodities = notifContext.ExistingImportNotification.Commodities?
            .Select(c => new
            {
                c.CommodityId,
                c.CommodityDescription
            }).ToList();
        var receivedCommodities = notifContext.PersistedImportNotification.Commodities?
            .Select(c => new
            {
                c.CommodityId,
                c.CommodityDescription
            }).ToList();

        if (existingCommodities?.Count != receivedCommodities?.Count ||
            existingCommodities?.TrueForAll(receivedCommodities!.Contains) != true)
        {
            // Delta in received Commodities
            return true;
        }

        return false;
    }

    private async Task<LinkResult> FindMovementLinks(Movement movement, CancellationToken cancellationToken)
    {
        var notifications = await dbContext.Notifications.Where(x => movement._MatchReferences.Contains(x._MatchReference)).ToListAsync(cancellationToken: cancellationToken);

        return new LinkResult(notifications.Any() ? LinkState.Linked : LinkState.NotLinked)
        {
            Movements = [movement],
            Notifications = notifications
        };
    }

    private async Task<LinkResult> FindImportNotificationLinks(ImportNotification importNotification, CancellationToken cancellationToken)
    {
        var movements = await dbContext.Movements.Where(x => x._MatchReferences.Contains(importNotification._MatchReference)).ToListAsync(cancellationToken);

        return new LinkResult(movements.Any() ? LinkState.Linked : LinkState.NotLinked)
        {
            Movements = movements,
            Notifications = [importNotification]
        };
    }
}