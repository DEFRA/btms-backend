using System.Diagnostics.Contracts;
using Btms.Model;
using Btms.Model.Cds;
using Btms.Model.Ipaffs;

namespace Btms.Analytics.Extensions;

public static class MovementExtensions
{   
    public static IQueryable<Movement>  WhereFilteredByCreatedDateAndParams(this IQueryable<Movement> source, DateTime from, DateTime to,
        bool finalisedOnly,
        ImportNotificationTypeEnum[]? chedTypes = null, string? country = null)
    {
        return source
            .Where(m => (m.CreatedSource >= from && m.CreatedSource < to)
                        && (country == null || m.DispatchCountryCode == country)
                        && (!finalisedOnly || m.Finalised.HasValue)
                        && (chedTypes == null || !chedTypes!.Any() ||
                            !m.BtmsStatus.ChedTypes!.Any() ||
                            m.BtmsStatus.ChedTypes!.Any(c => chedTypes!.Contains(c))));

    }

    public class ReadTimeDecisionStatusState<T>
    {
        public required Movement Movement { get; set; }
        public required ItemCheck Check { get; set; }
        public required T DecisionStatus { get; set; }
    }
    public static IQueryable<ReadTimeDecisionStatusState<DecisionStatusEnum>> WithReadTimeDecisionStatus(this IQueryable<ReadTimeDecisionStatusState<DecisionStatusEnum?>> source) {
        // blah-blah-blah
        //return something
        return source.Select(t => new ReadTimeDecisionStatusState<DecisionStatusEnum>() {
             Check   = t.Check,
             Movement = t.Movement,
             DecisionStatus = t.Movement.AlvsDecisionStatus.Context.DecisionComparison == null || t.Movement.AlvsDecisionStatus.Context.DecisionComparison.DecisionStatus == DecisionStatusEnum.InvestigationNeeded ?
                                 DecisionStatusEnum.InvestigationNeeded :
                                 // d.Movement.AlvsDecisionStatus.Context.DecisionComparison!.DecisionStatus :
                                 t.Movement.BtmsStatus.Segment == MovementSegmentEnum.Cdms205Ac1 ?
                                     DecisionStatusEnum.ReliesOnCDMS205 :
                                 t.Movement.BtmsStatus.Segment == MovementSegmentEnum.Cdms249 ?
                                     DecisionStatusEnum.ReliesOnCDMS249 :
                                 t.Movement.AlvsDecisionStatus.Context.DecisionComparison!.DecisionStatus ==
                                 DecisionStatusEnum.HasChedppChecks ?
                                     DecisionStatusEnum.HasChedppChecks :
                                 t.Check.BtmsDecisionCode == "E99" ? DecisionStatusEnum.HasGenericDataErrors :
                                 t.Check.BtmsDecisionCode != null && t.Check.BtmsDecisionCode.StartsWith("E9") ? DecisionStatusEnum.HasOtherDataErrors :
                                 t.Movement.BtmsStatus.ChedTypes.Length > 1 ? DecisionStatusEnum.HasMultipleChedTypes :
                                 t.Movement.Relationships.Notifications.Data.Count > 1 ? DecisionStatusEnum.HasMultipleCheds :
                                 t.Movement.AlvsDecisionStatus.Context.DecisionComparison!.DecisionStatus
        });
    }
    
    [Pure]
    public static DecisionStatusEnum GetDecisionStatus([Pure] ItemCheck? c)
    {
        return DecisionStatusEnum.None;
    }
    
    public class MovementWithLinkStatus
    {
        public required Movement Movement;
        public required DateTime CreatedSource;
        public required LinkStatusEnum Status;
    }
    
    public static IQueryable<MovementWithLinkStatus> SelectLinkStatus(this IQueryable<Movement> source)
    {
        var m = source
            .Select(m => new MovementWithLinkStatus() {
                Movement = m,
                CreatedSource = m.CreatedSource!.Value,
                Status = m.BtmsStatus.LinkStatus
            });

        return m;
    } 
}