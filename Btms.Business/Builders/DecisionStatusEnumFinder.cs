using Btms.Model;
using Btms.Model.Cds;
using Btms.Model.Ipaffs;

namespace Btms.Business.Builders;

public class DecisionStatusFinder
{
    private readonly List<(DecisionStatusEnum status, Func<Movement, AlvsDecision, bool> finder)> _finders = [];
    
    public DecisionStatusFinder()
    {
        _finders.Add((DecisionStatusEnum.BtmsMadeSameDecisionAsAlvs, BtmsMadeSameDecisionAsAlvs));
        _finders.Add((DecisionStatusEnum.BtmMadeSameDecisionTypeAsAlvs, BtmMadeSameDecisionTypeAsAlvs));
        _finders.Add((DecisionStatusEnum.NoImportNotificationsLinked, NoImportNotificationsLinked));
        _finders.Add((DecisionStatusEnum.NoAlvsDecisions, NoAlvsDecisions));
        _finders.Add((DecisionStatusEnum.HasChedppChecks, HasChedppChecks));
        
        _finders.Add((DecisionStatusEnum.AlvsX00CaseSensitivity, AlvsX00CaseSensitivity));
        _finders.Add((DecisionStatusEnum.AlvsX00WrongDocumentReferenceFormat, AlvsX00WrongDocumentReferenceFormat));
        _finders.Add((DecisionStatusEnum.AlvsX00NotBtms, AlvsX00NotBtms));
        _finders.Add((DecisionStatusEnum.ReliesOnCDMS205, ReliesOnCDMS205));
        _finders.Add((DecisionStatusEnum.ReliesOnCDMS249, ReliesOnCDMS249));
        _finders.Add((DecisionStatusEnum.HasOtherDataErrors, HasOtherDataErrors));
        _finders.Add((DecisionStatusEnum.HasGenericDataErrors, HasGenericDataErrors));
        _finders.Add((DecisionStatusEnum.HasMultipleChedTypes, HasMultipleChedTypes));
        _finders.Add((DecisionStatusEnum.HasMultipleCheds, HasMultipleCheds));
        
        _finders.Add((DecisionStatusEnum.InvestigationNeeded, InvestigationNeeded));
        
        // Default if none of the above match - none needs to be the last one in the Enum
        _finders.Add((DecisionStatusEnum.None, (m, d) => true));
        
        //Validate that each status in the enum has a finder
        var hasFinders = _finders.Select(f => f.status).ToArray();
        var needsFinders = Enum.GetValues<DecisionStatusEnum>();

        // var missing = needsFinders
        //     .Where(d => hasFinders.Any(h => h == d));
        
        var missing = needsFinders
            .Except(hasFinders);
        
        // var missing = hasFinders
        //     .Union(needsFinders)
        //     .Where(x => !hasFinders.Contains(x) || !needsFinders.Contains(x));

        if (missing.Count() > 0)
        {
            throw new Exception("Decision Status Finders missing in DecisionStatusFinder");
        }
    }

    public DecisionStatusEnum GetDecisionStatus(Movement movement, AlvsDecision decision)
    {
        return _finders
            .First(f => f.finder(movement, decision))
            .status;
    }

    private static bool BtmsMadeSameDecisionAsAlvs(Movement movement, AlvsDecision decision)
    {
        return decision.Context.DecisionComparison!.Checks.All(c => 
            c.AlvsDecisionCode == c.BtmsDecisionCode);
    }
    
    private static bool BtmMadeSameDecisionTypeAsAlvs(Movement movement, AlvsDecision decision)
    {
        return decision.Context.DecisionComparison!.Checks.All(c => 
            c.AlvsDecisionCode.First() == c.BtmsDecisionCode?.First());
    }
    
    private static bool NoImportNotificationsLinked(Movement movement, AlvsDecision decision)
    {
        return movement.Relationships.Notifications.Data.Count == 0;
    }
    
    private static bool NoAlvsDecisions(Movement movement, AlvsDecision decision)
    {
        return movement.AlvsDecisionStatus.Decisions.Count == 0;
    }
    
    private static bool HasChedppChecks(Movement movement, AlvsDecision decision)
    {
        return movement.BtmsStatus.ChedTypes.Contains(ImportNotificationTypeEnum.Chedpp);
    }
    
    private static bool AlvsX00CaseSensitivity(Movement movement, AlvsDecision decision)
    {
        return false;
    }
    
    private static bool AlvsX00WrongDocumentReferenceFormat(Movement movement, AlvsDecision decision)
    {
        return false;
    }
    
    private static bool AlvsX00NotBtms(Movement movement, AlvsDecision decision)
    {
        return false;
    }
    
    private static bool ReliesOnCDMS205(Movement movement, AlvsDecision decision)
    {
        return false;
    }
    
    private static bool ReliesOnCDMS249(Movement movement, AlvsDecision decision)
    {
        return false;
    }
    
    private static bool HasOtherDataErrors(Movement movement, AlvsDecision decision)
    {
        return false;
    }
    
    private static bool HasGenericDataErrors(Movement movement, AlvsDecision decision)
    {
        return false;
    }
    
    private static bool HasMultipleChedTypes(Movement movement, AlvsDecision decision)
    {
        return false;
    }
    
    private static bool HasMultipleCheds(Movement movement, AlvsDecision decision)
    {
        return false;
    }
    
    private static bool InvestigationNeeded(Movement movement, AlvsDecision decision)
    {
        return false;
    }
}