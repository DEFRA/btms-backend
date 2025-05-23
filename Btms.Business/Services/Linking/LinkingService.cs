using System.Text.RegularExpressions;
using Btms.Backend.Data;
using Btms.Backend.Data.Extensions;
using Btms.Business.Extensions;
using Btms.Metrics;
using Btms.Model;
using Btms.Model.Auditing;
using Btms.Model.ChangeLog;
using Btms.Model.Ipaffs;
using Btms.Model.Relationships;
using Microsoft.Extensions.Logging;

namespace Btms.Business.Services.Linking;

public static partial class LinkingServiceLogging
{
    [LoggerMessage(Level = LogLevel.Information, Message = "Linking Started for {ContextType} - {MatchIdentifier}")]
    internal static partial void LinkingStarted(this ILogger logger, string contextType, string matchIdentifier);

    [LoggerMessage(Level = LogLevel.Information, Message = "UnLinking Started for {ContextType} - {MatchIdentifier}")]
    internal static partial void UnLinkingStarted(this ILogger logger, string contextType, string matchIdentifier);

    [LoggerMessage(Level = LogLevel.Information, Message = "Linking Finished for {ContextType} - {MatchIdentifier}")]
    internal static partial void LinkingFinished(this ILogger logger, string contextType, string matchIdentifier);

    [LoggerMessage(Level = LogLevel.Information, Message = "UnLinking Finished for {ContextType} - {MatchIdentifier}")]
    internal static partial void UnLinkingFinished(this ILogger logger, string contextType, string matchIdentifier);

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
                        result = await FindMovementLinks(movementLinkContext.PersistedMovement, cancellationToken);

                        if (result.Notifications.All(x => x.Status == ImportNotificationStatusEnum.Deleted))
                        {
                            result.Outcome = LinkOutcome.NotLinked;
                            return result;
                        }

                        if (!ShouldLinkMovement(movementLinkContext.ChangeSet))
                        {
                            logger.LinkNotAttempted(linkContext.GetType().Name, linkContext.GetIdentifiers());
                            result.Outcome = LinkOutcome.LinksExist;
                            return result;
                        }

                        await CheckMovementForRemovedLinks(movementLinkContext, cancellationToken);

                        break;
                    case ImportNotificationLinkContext notificationLinkContext:
                        result = await FindImportNotificationLinks(notificationLinkContext.PersistedImportNotification, notificationLinkContext.Movements,
                            cancellationToken);

                        if (!ShouldLink(notificationLinkContext.ChangeSet))
                        {
                            logger.LinkNotAttempted(linkContext.GetType().Name, linkContext.GetIdentifiers());
                            result.Outcome = LinkOutcome.LinksExist;
                            return result;
                        }

                        break;
                    default: throw new ArgumentException("context type not supported");
                }

                if (result.Outcome == LinkOutcome.NotLinked)
                {
                    logger.LinkNotFound(linkContext.GetType().Name, linkContext.GetIdentifiers());
                    return result;
                }

                logger.LinkFound(linkContext.GetType().Name, linkContext.GetIdentifiers(), result.Movements.Count, result.Notifications.Count);

                metrics.Linked<Movement>(result.Movements.Count);
                metrics.Linked<ImportNotification>(result.Notifications.Count);

                foreach (var movement in result.Movements)
                {
                    foreach (var notification in result.Notifications)
                    {
                        notification.AddRelationship(new TdmRelationshipObject
                        {
                            Links = RelationshipLinks.CreateForNotification(notification),
                            Data =
                            [
                                RelationshipDataItem.CreateFromMovement(notification, movement,
                                    notification._MatchReference)
                            ]
                        });

                        movement.AddRelationship(new TdmRelationshipObject
                        {
                            Links = RelationshipLinks.CreateForMovement(movement),
                            Data =
                            [
                                RelationshipDataItem.CreateFromNotification(notification, movement,
                                    notification._MatchReference)
                            ]
                        });


                        await dbContext.Notifications.Update(notification, cancellationToken: cancellationToken);

                    }

                    await dbContext.Movements.Update(movement, cancellationToken: cancellationToken);
                }

            }
            catch (Exception e)
            {
                // No Exception is logged at this point, as its logged further up the stack
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

    public async Task UnLink(ImportNotificationLinkContext linkContext, CancellationToken cancellationToken = default)
    {
        var startedAt = TimeProvider.System.GetTimestamp();
        using (logger.BeginScope(new List<KeyValuePair<string, object>>
               {
                   new("MatchIdentifier", linkContext.GetIdentifiers()),
                   new("ContextType", linkContext.GetType().Name),
               }))
        {
            logger.UnLinkingStarted(linkContext.GetType().Name, linkContext.GetIdentifiers());
            try
            {
                var result = await FindImportNotificationLinks(linkContext.PersistedImportNotification, linkContext.Movements, cancellationToken);

                foreach (var movement in result.Movements)
                {
                    await RemoveNotificationLinkFromMovement(movement,
                        linkContext.PersistedImportNotification._MatchReference, cancellationToken);
                    await dbContext.Movements.Update(movement, cancellationToken: cancellationToken);
                }

                linkContext.PersistedImportNotification.RemoveAllRelationships();

                await dbContext.Notifications.Update(linkContext.PersistedImportNotification, cancellationToken: cancellationToken);

            }
            catch (Exception e)
            {
                // No Exception is logged at this point, as its logged further up the stack
                metrics.Faulted(e);
                throw new LinkException(e);
            }
            finally
            {
                var e = TimeProvider.System.GetElapsedTime(startedAt);
                metrics.Completed(e.TotalMilliseconds);
                logger.UnLinkingFinished(linkContext.GetType().Name, linkContext.GetIdentifiers());
            }
        }
    }

    private async Task CheckMovementForRemovedLinks(MovementLinkContext linkContext,
        CancellationToken cancellationToken = default)
    {
        var chedRefs = linkContext.ChangeSet?.GetPreviousValue<List<string>>($"{nameof(Movement._MatchReferences)}");

        if (chedRefs?.Count > 0)
        {
            var removedRefs = chedRefs.Except(linkContext.PersistedMovement._MatchReferences).ToList();
            if (removedRefs.Any())
            {
                foreach (var chedRef in chedRefs)
                {
                    await RemoveNotificationLinkFromMovement(linkContext.PersistedMovement, chedRef,
                        cancellationToken);
                    await RemoveMovementLinkFromNotification(linkContext.PersistedMovement.Id, chedRef,
                        cancellationToken);
                }
            }
        }
    }

    private async Task RemoveNotificationLinkFromMovement(Movement movement, string chedRef,
        CancellationToken cancellationToken = default)
    {
        var notification = dbContext.Notifications.SingleOrDefault(x => x._MatchReference == chedRef);

        if (notification != null)
        {
            var relationshipLink = movement.Relationships.Notifications.Data?
                .SingleOrDefault(x => x.Id == notification.Id && x.Type == "notifications");

            if (relationshipLink != null)
            {
                movement.RemoveRelationship(relationshipLink);
                await dbContext.Movements.Update(movement, cancellationToken: cancellationToken);
            }
        }
    }

    private async Task RemoveMovementLinkFromNotification(string? movementId, string chedRef,
        CancellationToken cancellationToken = default)
    {
        var notification = dbContext.Notifications.SingleOrDefault(x => x._MatchReference == chedRef);

        if (notification != null)
        {
            var relationshipLink = notification.Relationships.Movements.Data?
                .SingleOrDefault(x => x.Id == movementId && x.Type == "movements");

            if (relationshipLink != null)
            {
                notification.RemoveRelationship(relationshipLink);

                await dbContext.Notifications.Update(notification, cancellationToken: cancellationToken);
            }
        }
    }

    private static bool ShouldLinkMovement(ChangeSet? changeSet)
    {
        return changeSet is null || changeSet.HasDocumentsChanged();
    }

    private static bool ShouldLink(ChangeSet? changeSet)
    {
        return changeSet is null || changeSet.HasCommoditiesChanged();
    }

    private async Task<LinkResult> FindMovementLinks(Movement movement, CancellationToken cancellationToken)
    {
        var notifications = await dbContext.Notifications.Where(x => movement._MatchReferences.Contains(x._MatchReference) && x.Status != ImportNotificationStatusEnum.Deleted)
            .ToListAsync(cancellationToken: cancellationToken);

        return new LinkResult(notifications.Any() ? LinkOutcome.Linked : LinkOutcome.NotLinked)
        {
            Movements = [movement],
            Notifications = notifications
        };
    }

    private async Task<LinkResult> FindImportNotificationLinks(ImportNotification importNotification, List<Movement>? movements, CancellationToken cancellationToken)
    {
        if (movements is null || !movements.Any())
        {
            movements = await dbContext.Movements
                .Where(x => x._MatchReferences.Contains(importNotification._MatchReference))
                .ToListAsync(cancellationToken);
        }

        return new LinkResult(movements.Any() ? LinkOutcome.Linked : LinkOutcome.NotLinked)
        {
            Movements = movements,
            Notifications = [importNotification]
        };
    }
}

public static partial class LinkingChangeSetExtensions
{
    [GeneratedRegex("Commodities\\/\\d\\/CommodityId", RegexOptions.IgnoreCase)]
    private static partial Regex CommodityIdRegex();

    [GeneratedRegex("Commodities\\/\\d", RegexOptions.IgnoreCase)]
    private static partial Regex CommodityRegex();

    [GeneratedRegex("Items\\/\\d", RegexOptions.IgnoreCase)]
    private static partial Regex ItemsRegex();

    [GeneratedRegex("Items\\/\\d\\/Documents\\/\\d", RegexOptions.IgnoreCase)]
    private static partial Regex DocumentsRegex();

    public static bool HasCommoditiesChanged(this ChangeSet changeSet)
    {
        return changeSet.JsonPatch.Operations
            .Any(x => CommodityIdRegex().IsMatch(x.Path.ToString())
                      || CommodityRegex().IsMatch(x.Path.ToString()));
    }

    public static bool HasDocumentsChanged(this ChangeSet changeSet)
    {
        return changeSet.JsonPatch.Operations
            .Any(x => ItemsRegex().IsMatch(x.Path.ToString())
                      || DocumentsRegex().IsMatch(x.Path.ToString()));
    }
}