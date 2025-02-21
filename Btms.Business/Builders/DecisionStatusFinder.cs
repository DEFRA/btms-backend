using Btms.Model;
using Btms.Model.Cds;
using Btms.Model.Ipaffs;

namespace Btms.Business.Builders;

public class DecisionStatusFinder
{
    private readonly Dictionary<DecisionStatusEnum, Func<Movement, AlvsDecision, bool>> _finders = [];
    private readonly List<DecisionStatusEnum> _orderedFinders = Enum.GetValues<DecisionStatusEnum>().ToList();
    public DecisionStatusFinder()
    {
        _finders.Add(DecisionStatusEnum.BtmsMadeSameDecisionAsAlvs, BtmsMadeSameDecisionAsAlvs);
        _finders.Add(DecisionStatusEnum.BtmMadeSameDecisionTypeAsAlvs, BtmMadeSameDecisionTypeAsAlvs);
        _finders.Add(DecisionStatusEnum.NoImportNotificationsLinked, NoImportNotificationsLinked);
        _finders.Add(DecisionStatusEnum.PartialImportNotificationsLinked, PartialImportNotificationsLinked);
        _finders.Add(DecisionStatusEnum.NoAlvsDecisions, NoAlvsDecisions);
        _finders.Add(DecisionStatusEnum.HasChedppChecks, HasChedppChecks);

        _finders.Add(DecisionStatusEnum.DocumentReferenceCaseIncorrect, DocumentReferenceCaseIncorrect);
        _finders.Add(DecisionStatusEnum.DocumentReferenceFormatIncorrect, DocumentReferenceFormatIncorrect);
        _finders.Add(DecisionStatusEnum.AlvsX00NotBtms, AlvsX00NotBtms);

        _finders.Add(DecisionStatusEnum.ReliesOnCDMS205, ReliesOnCDMS205);
        _finders.Add(DecisionStatusEnum.ReliesOnCDMS249, ReliesOnCDMS249);
        _finders.Add(DecisionStatusEnum.HasOtherDataErrors, HasOtherDataErrors);
        _finders.Add(DecisionStatusEnum.HasGenericDataErrors, HasGenericDataErrors);
        _finders.Add(DecisionStatusEnum.HasMultipleChedTypes, HasMultipleChedTypes);
        _finders.Add(DecisionStatusEnum.BtmsClearAlvsHold, BtmsClearAlvsHold);
        _finders.Add(DecisionStatusEnum.AlvsClearBtmsHold, AlvsClearBtmsHold);
        _finders.Add(DecisionStatusEnum.HasMultipleCheds, HasMultipleCheds);

        _finders.Add(DecisionStatusEnum.InvestigationNeeded, InvestigationNeeded);

        // Default if none of the above match - none needs to be the last one in the Enum
        _finders.Add(DecisionStatusEnum.None, (m, d) => true);

        //Validate that each status in the enum has a finder
        var hasFinders = _finders.Select(f => f.Key).ToArray();

        var missing = _orderedFinders
            .Except(hasFinders);

        if (missing.Any())
        {
            throw new InvalidOperationException("Decision Status Finders missing in DecisionStatusFinder");
        }
    }

    public DecisionStatusEnum GetStatus(Movement movement, AlvsDecision decision)
    {
        return _orderedFinders
            .First(f => _finders[f](movement, decision));
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

    private static bool PartialImportNotificationsLinked(Movement movement, AlvsDecision decision)
    {
        return movement.Relationships.Notifications.Data.Count > 0 &&
               movement.Relationships.Notifications.Data.Count < movement._MatchReferences.Count;
    }

    private static bool NoAlvsDecisions(Movement movement, AlvsDecision decision)
    {
        return movement.AlvsDecisionStatus.Decisions.Count == 0;
    }

    private static bool HasChedppChecks(Movement movement, AlvsDecision decision)
    {
        return movement.BtmsStatus.ChedTypes.Contains(ImportNotificationTypeEnum.Chedpp);
    }

    private static bool DocumentReferenceFormatIncorrect(Movement movement, AlvsDecision? decision)
    {
        return movement.Items.Any(i =>
            i.Documents?
                .Any(d => !MatchIdentifier.TryFromCds(d.DocumentReference!, out _)) ?? false);
    }

    private static bool DocumentReferenceCaseIncorrect(Movement movement, AlvsDecision decision)
    {
        return movement.Items.Any(i =>
            i.Documents?.Any(d => d.DocumentReference != d.DocumentReference?.ToUpper()) ?? false);
    }

    private static bool AlvsX00NotBtms(Movement movement, AlvsDecision decision)
    {
        return movement.AlvsDecisionStatus.Context.DecisionComparison!.Checks.Any(c =>
            c.AlvsDecisionCode == "X00" && c.BtmsDecisionCode != "X00");
    }

    private static bool ReliesOnCDMS205(Movement movement, AlvsDecision decision)
    {
        return movement.BtmsStatus.Segment == MovementSegmentEnum.Cdms205Ac1;
    }

    private static bool ReliesOnCDMS249(Movement movement, AlvsDecision decision)
    {
        return movement.BtmsStatus.Segment == MovementSegmentEnum.Cdms249;
    }

    private static bool HasGenericDataErrors(Movement movement, AlvsDecision decision)
    {
        return movement.Items.Any(i => i.Checks?.Any(c => c.DecisionCode == "E99") ?? false);
    }

    private static bool HasOtherDataErrors(Movement movement, AlvsDecision decision)
    {
        return movement.Items.Any(i => i.Checks?.Any(c => c.DecisionCode?.StartsWith("E9") ?? false) ?? false);
    }

    private static bool HasMultipleChedTypes(Movement movement, AlvsDecision decision)
    {
        return movement.BtmsStatus.ChedTypes.Count() > 1;
    }

    private static bool HasMultipleCheds(Movement movement, AlvsDecision decision)
    {
        return movement.Relationships.Notifications.Data.Count > 1;
    }

    private static bool BtmsClearAlvsHold(Movement movement, AlvsDecision decision)
    {
        return movement.AlvsDecisionStatus?.Context.DecisionComparison?.Checks.Any(c =>
            (c.BtmsDecisionCode?.StartsWith('C') ?? false) &&
            (c.AlvsDecisionCode?.StartsWith('H') ?? false)
        ) ?? false;
    }

    private static bool AlvsClearBtmsHold(Movement movement, AlvsDecision decision)
    {
        return movement.AlvsDecisionStatus?.Context.DecisionComparison?.Checks.Any(c =>
            (c.BtmsDecisionCode?.StartsWith('H') ?? false) &&
            (c.AlvsDecisionCode?.StartsWith('C') ?? false)
        ) ?? false;
    }

    private static bool InvestigationNeeded(Movement movement, AlvsDecision decision)
    {
        return true;
    }
}