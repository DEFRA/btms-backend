using System.Text.Json;
using Btms.Common.Extensions;
using Btms.Model;
using Btms.Types.Alvs;
using TestDataGenerator.Helpers;
using Finalisation = Btms.Types.Alvs.Finalisation;

namespace TestDataGenerator;

public class FinalisationBuilder(string file) : FinalisationBuilder<Btms.Types.Alvs.Finalisation>(file);

public class FinalisationBuilder<T> : BuilderBase<T, FinalisationBuilder<T>>
    where T : Finalisation
{

    protected FinalisationBuilder(string? file = null, string? itemJson = null) : base(file, itemJson)
    {
    }

    public static FinalisationBuilder<T> Default()
    {
        return new FinalisationBuilder<T>();
    }

    public static FinalisationBuilder<T> FromFile(string file)
    {
        return new FinalisationBuilder<T>(file);
    }
    
    /// <summary>
    /// build, serialise and then deserialise the object to break any byref type relationships
    /// </summary>
    /// <returns></returns>
    public FinalisationBuilder<T> Clone()
    {

        var json = JsonSerializer.Serialize(this.Build());
        
        var builder =  new FinalisationBuilder<T>(itemJson: json);
        
        return builder;
    }

    public FinalisationBuilder<T> WithReferenceNumber(string chedReference)
    {
        var id = MatchIdentifier.FromNotification(chedReference);
        return WithReferenceNumber(id);   
    }
    
    public FinalisationBuilder<T> WithReferenceNumber(MatchIdentifier id)
    {
        return Do(x =>
        {
            x.Header!.EntryReference = id.AsCdsEntryReference();
        });   
    }

    public FinalisationBuilder<T> WithCreationDate(DateTime entryDate, bool randomTime = true)
    {
        if (randomTime)
        {
            var hour = entryDate.Date == DateTime.Today ? DateTime.Now.AddHours(-2).Hour : 23;
            return Do(x => x.ServiceHeader!.ServiceCallTimestamp = entryDate.RandomTime(hour));
        }
        else
        {
            return Do(x => x.ServiceHeader!.ServiceCallTimestamp = entryDate);
        }
    }
    
    public FinalisationBuilder<T> WithEntryVersionNumber(int version = 1)
    {
        return Do(x => x.Header!.EntryVersionNumber = version);
    }
    
    public FinalisationBuilder<T> WithDecisionVersionNumber(int version = 1)
    {
        return Do(x => x.Header!.DecisionNumber = version);
    }

    protected override FinalisationBuilder<T> Validate()
    {
        return Do(cr =>
        {
            cr.ServiceHeader!.ServiceCallTimestamp.AssertHasValue("Finalisation ServiceCallTimestamp missing");
            cr.Header!.EntryReference.AssertHasValue("Finalisation EntryReference missing");
            cr.Header!.DecisionNumber.AssertHasValue("Finalisation DecisionNumber missing");
            cr.Header!.FinalState.AssertHasValue("Finalisation FinalState missing");
            cr.Header!.ManualAction.AssertHasValue("Finalisation ManualAction missing");
        });
    }
}