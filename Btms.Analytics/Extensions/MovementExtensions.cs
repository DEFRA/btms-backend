using System.Diagnostics.CodeAnalysis;
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
        public required Movement Movement { get; init; }
        public required ItemCheck Check { get; init; }
        public required T DecisionStatus { get; init; }
    }
    
    [SuppressMessage("SonarLint", "S3358",
        Justification =
            "This is a linq expression tree, unsure how to make it independent expressions"), SuppressMessage("SonarLint", "S3776",
         Justification =
             "Unsure how to make this less complex as its a linq expression")]
    public static IQueryable<ReadTimeDecisionStatusState<DecisionStatusEnum>> WithReadTimeDecisionStatus(this IQueryable<ReadTimeDecisionStatusState<DecisionStatusEnum?>> source) {
        
        return source.Select(t => new ReadTimeDecisionStatusState<DecisionStatusEnum>() {
             Check   = t.Check,
             Movement = t.Movement,
             
             DecisionStatus = t.Movement.AlvsDecisionStatus.Context.DecisionComparison == null || t.Movement.AlvsDecisionStatus.Context.DecisionComparison.DecisionStatus == DecisionStatusEnum.InvestigationNeeded ?
                                 DecisionStatusEnum.InvestigationNeeded :
                                 // d.Movement.AlvsDecisionStatus.Context.DecisionComparison!.DecisionStatus :
                                 // t.Movement.AlvsDecisionStatus.Context.DecisionComparison.
                                 t.Movement.AlvsDecisionStatus.Context.DecisionComparison.Checks.Any(c => c.AlvsDecisionCode == "X00" && c.BtmsDecisionCode != "X00") ? DecisionStatusEnum.AlvsX00NotBtms :
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
        public required Movement Movement { get; set; }
        public required DateTime CreatedSource { get; set; }
        public required LinkStatusEnum Status { get; set; }
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