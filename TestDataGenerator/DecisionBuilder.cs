using System.Text.Json;
using Btms.Common.Extensions;
using Btms.Model;
using Btms.Types.Alvs;
using TestDataGenerator.Helpers;

namespace TestDataGenerator;

public class DecisionBuilder(string file) : DecisionBuilder<Btms.Types.Alvs.Decision>(file);

public class DecisionBuilder<T> : BuilderBase<T, DecisionBuilder<T>>
    where T : Decision, new()
{

    protected DecisionBuilder(string? file = null, string? itemJson = null) : base(file, itemJson)
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

    /// <summary>
    /// build, serialise and then deserialise the object to break any byref type relationships
    /// </summary>
    /// <returns></returns>
    public DecisionBuilder<T> Clone()
    {

        var json = JsonSerializer.Serialize(this.Build());

        var builder = new DecisionBuilder<T>(itemJson: json);

        return builder;
    }

    public DecisionBuilder<T> WithReferenceNumber(string chedReference)
    {
        var id = MatchIdentifier.FromNotification(chedReference);
        return WithReferenceNumber(id);
    }

    public DecisionBuilder<T> WithReferenceNumber(MatchIdentifier id)
    {
        return Do(x =>
        {
            x.Header!.EntryReference = id.AsCdsEntryReference();
            x.Header!.DeclarationUcr = id.AsCdsDeclarationUcr();
            x.Header!.MasterUcr = id.AsCdsMasterUcr();
        });
    }

    public DecisionBuilder<T> WithCreationDate(DateTime entryDate, bool randomTime = true)
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


    public DecisionBuilder<T> WithEntryVersionNumber(int version = 1)
    {
        return Do(x => x.Header!.EntryVersionNumber = version);
    }
    public DecisionBuilder<T> WithDecisionVersionNumber(int version = 1)
    {
        return Do(x => x.Header!.DecisionNumber = version);
    }

    public DecisionBuilder<T> WithItemAndCheck(int item, string checkCode, string decisionCode)
    {
        return Do(dec =>
        {
            if (!dec.Items.HasValue())
            {
                dec.Items = [];
            }

            dec.Items = dec
                .Items
                .Append(new DecisionItems()
                {
                    ItemNumber = item,
                    Checks = new[] { new DecisionCheck() { CheckCode = checkCode, DecisionCode = decisionCode } }
                })
                .ToArray();
        });
    }

    protected override DecisionBuilder<T> Validate()
    {
        return Do(cr =>
        {
            cr.ServiceHeader!.ServiceCallTimestamp.AssertHasValue("Decision ServiceCallTimestamp missing");
            cr.Header!.EntryReference.AssertHasValue("Decision EntryReference missing");
            cr.Header!.DecisionNumber.AssertHasValue("Decision DecisionNumber missing");

            cr.Items.AssertHasValue("Decision Items missing");

            Array.ForEach(cr.Items!, i =>
            {
                i.Checks.AssertHasValue("Decision Item Checks missing");

                Array.ForEach(i.Checks!, c =>
                {
                    c.CheckCode.AssertHasValue("Decision Item Check CheckCode missing");
                    c.DecisionCode.AssertHasValue("Decision Item Check DecisionCode missing");
                });
            });
        });
    }
}