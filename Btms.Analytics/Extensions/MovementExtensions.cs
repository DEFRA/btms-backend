using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using Btms.Model;
using Btms.Model.Cds;
using Btms.Model.Ipaffs;
using Microsoft.Extensions.Logging;
using LinkStatus = Btms.Model.Cds.LinkStatus;

namespace Btms.Analytics.Extensions;

public static class MovementExtensions
{
    public static IQueryable<Movement> WhereFilteredByCreatedDateAndParams(this IQueryable<Movement> source, DateTime from, DateTime to,
        bool finalisedOnly,
        ImportNotificationTypeEnum[]? chedTypes = null, string? country = null)
    {
        return source
            .Where(m => (m.CreatedSource >= from && m.CreatedSource < to)
                        && (country == null || m.DispatchCountryCode == country)
                        && (!finalisedOnly || m.Finalised.HasValue)
                        && (chedTypes == null || !chedTypes!.Any() ||
                            !m.Status.ChedTypes!.Any() ||
                            m.Status.ChedTypes!.Any(c => chedTypes!.Contains(c))));

    }

    public class ReadTimeDecisionStatusState<T>
    {
        public required Movement Movement { get; init; }
        public required ItemCheck Check { get; init; }
        public required T DecisionStatus { get; init; }
    }

    [SuppressMessage("SonarLint", "S3358",
        Justification =
            "This is a linq expression tree, unsure how to make it independent expressions"), SuppressMessage("SonarLint", "S3776",
         Justification =
             "Unsure how to make this less complex as its a linq expression")]
    public static IQueryable<ReadTimeDecisionStatusState<DecisionStatusEnum>> WithReadTimeDecisionStatus(this IQueryable<ReadTimeDecisionStatusState<DecisionStatusEnum?>> source)
    {

        return source.Select(t => new ReadTimeDecisionStatusState<DecisionStatusEnum>()
        {
            Check = t.Check,
            Movement = t.Movement,

            DecisionStatus = t.Movement.AlvsDecisionStatus.Context.DecisionComparison == null ?
                 DecisionStatusEnum.InvestigationNeeded :
                 t.Movement.AlvsDecisionStatus.Context.DecisionComparison.DecisionStatus
        });
    }

    [Pure]
    public static DecisionStatusEnum GetDecisionStatus([Pure] ItemCheck? c)
    {
        return DecisionStatusEnum.None;
    }

    public class MovementWithLinkStatus
    {
        public required Movement Movement { get; set; }
        public required DateTime CreatedSource { get; set; }
        public required LinkStatus Status { get; set; }
    }

    public static IQueryable<MovementWithLinkStatus> SelectLinkStatus(this IQueryable<Movement> source)
    {
        var m = source
            .Select(m => new MovementWithLinkStatus()
            {
                Movement = m,
                CreatedSource = m.CreatedSource!.Value,
                Status = m.Status.LinkStatus
            });

        return m;
    }
}