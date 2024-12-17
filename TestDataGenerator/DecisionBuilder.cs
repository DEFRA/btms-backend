using Btms.Common.Extensions;
using Btms.Model;
using Btms.Types.Alvs;
using TestDataGenerator.Helpers;

namespace TestDataGenerator;

public class DecisionBuilder(string file) : DecisionBuilder<AlvsClearanceRequest>(file);

public class DecisionBuilder<T> : BuilderBase<T, DecisionBuilder<T>>
    where T : AlvsClearanceRequest, new()
{
    private DecisionBuilder()
    {
    }

    protected DecisionBuilder(string file) : base(file)
    {
    }

    public static DecisionBuilder<T> Default()
    {
        return new DecisionBuilder<T>();
    }

    public static DecisionBuilder<T> FromFile(string file)
    {
        return new DecisionBuilder<T>(file);
    }

    public DecisionBuilder<T> WithReferenceNumber(string chedReference)
    {
        var id = MatchIdentifier.FromNotification(chedReference);
        return Do(x =>
        {
            x.Header!.EntryReference = id.AsCdsEntryReference();
            x.Header!.DeclarationUcr = id.AsCdsDeclarationUcr();
            x.Header!.MasterUcr = id.AsCdsMasterUcr();
        });   
    }

    public DecisionBuilder<T> WithDecisionNumber(int number)
    {
        return Do(x => x.Header!.DecisionNumber = number);
    }

    public DecisionBuilder<T> WithEntryDate(DateTime entryDate)
    {
        return Do(x => x.ServiceHeader!.ServiceCallTimestamp = entryDate.RandomTime());
    }

    public DecisionBuilder<T> WithItemAndCheck(int item, string checkCode, string decisionCode)
    {
        return Do(dec =>
        {
            dec.Items![0].ItemNumber = item;
            dec.Items![0].Checks![0].CheckCode = checkCode;
            dec.Items![0].Checks![0].DecisionCode = decisionCode;
        });
    }

    protected override DecisionBuilder<T> Validate()
    {
        return Do(cr =>
        {
            cr.ServiceHeader!.ServiceCallTimestamp.AssertHasValue("Decision ServiceCallTimestamp missing");
            cr.Header!.EntryReference.AssertHasValue("Decision EntryReference missing");
            cr.Header!.DecisionNumber.AssertHasValue("Decision DecisionNumber missing");

            Array.ForEach(cr.Items!, i => 
                Array.ForEach(i.Checks!, c =>
                {
                    c.CheckCode.AssertHasValue();
                    c.DecisionCode.AssertHasValue();
                }));
        });
    }
}