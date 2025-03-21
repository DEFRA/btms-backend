using Btms.Common.Extensions;
using Btms.Model;
using Btms.Model.Cds;
using Btms.Model.Ipaffs;

namespace Btms.Business.Builders;

public static class BusinessDecisionStatusFinder
{
    private static readonly Dictionary<BusinessDecisionStatusEnum, Func<Movement, bool>> _finders = [];
    private static readonly List<BusinessDecisionStatusEnum> _orderedFinders = Enum.GetValues<BusinessDecisionStatusEnum>().ToList();
    static BusinessDecisionStatusFinder()
    {
        _finders.Add(BusinessDecisionStatusEnum.CancelledOrDestroyed, CancelledOrDestroyed);
        _finders.Add(BusinessDecisionStatusEnum.ManualReleases, ManualReleases);
        _finders.Add(BusinessDecisionStatusEnum.MatchComplete, MatchComplete);
        _finders.Add(BusinessDecisionStatusEnum.MatchGroup, MatchGroup);
        _finders.Add(BusinessDecisionStatusEnum.AlvsHoldBtmsNotHeld, AlvsHoldBtmsNotHeld);
        _finders.Add(BusinessDecisionStatusEnum.AlvsNotHeldBtmsHold, AlvsNotHeldBtmsHold);
        _finders.Add(BusinessDecisionStatusEnum.AlvsReleaseBtmsNotReleased, AlvsReleaseBtmsNotReleased);
        _finders.Add(BusinessDecisionStatusEnum.AlvsNotReleasedBtmsReleased, AlvsNotReleasedBtmsReleased);
        _finders.Add(BusinessDecisionStatusEnum.AlvsRefuseBtmsNotRefused, AlvsRefuseBtmsNotRefused);
        _finders.Add(BusinessDecisionStatusEnum.AlvsNotRefusedBtmsRefused, AlvsNotRefusedBtmsRefused);
        _finders.Add(BusinessDecisionStatusEnum.AlvsDataErrorDecision, AlvsDataErrorDecision);
        _finders.Add(BusinessDecisionStatusEnum.BtmsDataErrorDecision, BtmsDataErrorDecision);

        // Default if none of the above match - AnythingElse needs to be the last one in the Enum
        _finders.Add(BusinessDecisionStatusEnum.AnythingElse, _ => true);

        Validate();
    }

    internal static void Validate()
    {
        //Validate that each status in the enum has a finder
        var hasFinders = _finders.Select(f => f.Key).ToArray();

        var missing = _orderedFinders
            .Except(hasFinders);

        if (missing.Any())
        {
            throw new InvalidOperationException("Decision Status Finders missing in DecisionStatusFinder");
        }
    }

    public static BusinessDecisionStatusEnum GetBusinessDecisionStatus(this Movement movement)
    {
        try
        {
            return _orderedFinders
                .First(f => _finders[f](movement));
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return BusinessDecisionStatusEnum.AnythingElse;
        }
    }

    private static readonly FinalState[] cancelledOrDestroyed = [
        FinalState.CancelledAfterArrival,
        FinalState.CancelledWhilePreLodged,
        FinalState.Destroyed,
        FinalState.Seized,
        FinalState.ReleasedToKingsWarehouse,
        FinalState.TransferredToMss
    ];

    private static bool CancelledOrDestroyed(Movement movement)
    {
        return movement.Finalisation.HasValue() && cancelledOrDestroyed.Contains(movement.Finalisation!.FinalState);
    }

    private static bool ManualReleases(Movement movement)
    {
        return movement.Finalisation?.ManualAction == true;
    }

    private static bool MatchComplete(Movement movement)
    {
        return movement.AlvsDecisionStatus.Context.DecisionComparison?.DecisionStatus ==
               DecisionStatusEnum.BtmsMadeSameDecisionAsAlvs;
    }

    private static bool MatchGroup(Movement movement)
    {
        return movement.AlvsDecisionStatus.Context.DecisionComparison?.DecisionStatus ==
               DecisionStatusEnum.BtmMadeSameDecisionTypeAsAlvs;
    }

    private static bool AlvsHoldBtmsNotHeld(Movement movement)
    {
        return movement.AlvsDecisionStatus.Context.DecisionComparison?.Checks.Any(c =>
            c.AlvsDecisionCode.StartsWith('H') &&
            !(c.BtmsDecisionCode?.StartsWith('H') ?? false)) ?? false;
    }

    private static bool AlvsNotHeldBtmsHold(Movement movement)
    {
        return movement.AlvsDecisionStatus.Context.DecisionComparison?.Checks.Any(c =>
            !c.AlvsDecisionCode.StartsWith('H') &&
            (c.BtmsDecisionCode?.StartsWith('H') ?? false)) ?? false;
    }

    private static bool AlvsReleaseBtmsNotReleased(Movement movement)
    {
        return movement.AlvsDecisionStatus.Context.DecisionComparison?.Checks.Any(c =>
            c.AlvsDecisionCode.StartsWith('C') &&
            !(c.BtmsDecisionCode?.StartsWith('C') ?? false)) ?? false;
    }

    private static bool AlvsNotReleasedBtmsReleased(Movement movement)
    {
        return movement.AlvsDecisionStatus.Context.DecisionComparison?.Checks.Any(c =>
            !c.AlvsDecisionCode.StartsWith('C') &&
            (c.BtmsDecisionCode?.StartsWith('C') ?? false)) ?? false;
    }

    private static bool AlvsRefuseBtmsNotRefused(Movement movement)
    {
        return movement.AlvsDecisionStatus.Context.DecisionComparison?.Checks.Any(c =>
            c.AlvsDecisionCode.StartsWith('N') &&
            !(c.BtmsDecisionCode?.StartsWith('N') ?? false)) ?? false;
    }

    private static bool AlvsNotRefusedBtmsRefused(Movement movement)
    {
        return movement.AlvsDecisionStatus.Context.DecisionComparison?.Checks.Any(c =>
            !c.AlvsDecisionCode.StartsWith('N') &&
            (c.BtmsDecisionCode?.StartsWith('N') ?? false)) ?? false;
    }

    private static bool AlvsDataErrorDecision(Movement movement)
    {
        return movement.AlvsDecisionStatus.Context.DecisionComparison?.Checks.Any(c =>
            c.AlvsDecisionCode.StartsWith('E')) ?? false;
    }

    private static bool BtmsDataErrorDecision(Movement movement)
    {
        return movement.AlvsDecisionStatus.Context.DecisionComparison?.Checks.Any(c =>
            c.BtmsDecisionCode?.StartsWith('E') ?? false) ?? false;
    }
}