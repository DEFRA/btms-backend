using System.Text.Json;
using Btms.Common.Extensions;
using Btms.Model;
using Btms.Types.Alvs;
using TestDataGenerator.Helpers;

namespace TestDataGenerator;

public class ClearanceRequestBuilder(string file) : ClearanceRequestBuilder<AlvsClearanceRequest>(file);

public class ClearanceRequestBuilder<T> : BuilderBase<T, ClearanceRequestBuilder<T>>
    where T : AlvsClearanceRequest, new()
{
    protected ClearanceRequestBuilder(string? file = null, string? itemJson = null) : base(file, itemJson)
    {
    }

    public static ClearanceRequestBuilder<T> Default()
    {
        return new ClearanceRequestBuilder<T>();
    }

    public static ClearanceRequestBuilder<T> FromFile(string file)
    {
        return new ClearanceRequestBuilder<T>(file);
    }


    /// <summary>
    /// build, serialise and then deserialise the object to break any byref type relationships
    /// </summary>
    /// <returns></returns>
    public ClearanceRequestBuilder<T> Clone()
    {

        var json = JsonSerializer.Serialize(this.Build());

        var builder = new ClearanceRequestBuilder<T>(itemJson: json);

        return builder;
    }

    public ClearanceRequestBuilder<T> WithReferenceNumberOneToOne(string chedReference)
    {
        var id = MatchIdentifier.FromNotification(chedReference);
        var clearanceRequestDocumentReference = id.AsCdsDocumentReference();

        return
            Do(x =>
            {
                x.Header!.EntryReference = id.AsCdsEntryReference();
                x.Header!.DeclarationUcr = id.AsCdsDeclarationUcr();
                x.Header!.MasterUcr = id.AsCdsMasterUcr();
                Array.ForEach(x.Items!, i =>
                    Array.ForEach(i.Documents!, d => d.DocumentReference = clearanceRequestDocumentReference));
            });
    }

    public ClearanceRequestBuilder<T> IncrementCreationDate(TimeSpan t)
    {
        return Do(x => x.ServiceHeader!.ServiceCallTimestamp = x.ServiceHeader!.ServiceCallTimestamp!.Value.Add(t));
    }

    public ClearanceRequestBuilder<T> WithDispatchCountryCode(string? countryCode)
    {
        return Do(x => x.Header!.DispatchCountryCode = countryCode);
    }

    public ClearanceRequestBuilder<T> WithCreationDate(DateTime entryDate, bool randomTime = true)
    {
        var entry = randomTime ?
            // We don't want documents created in the future!
            entryDate.RandomTime(entryDate.Date == DateTime.Today ? DateTime.Now.AddHours(-2).Hour : 23)
            : entryDate;

        return Do(x => x.ServiceHeader!.ServiceCallTimestamp = entry);
    }
    public ClearanceRequestBuilder<T> WithEntryVersionNumber(int version = 1)
    {
        return Do(x =>
        {
            x.Header!.EntryVersionNumber = version;

            if (version > 1)
            {
                x.Header!.PreviousVersionNumber = version - 1;
            }
        });
    }

    public ClearanceRequestBuilder<T> IncrementEntryVersionNumber()
    {
        return Do(x =>
        {
            x.Header!.PreviousVersionNumber = x.Header!.EntryVersionNumber;
            x.Header!.EntryVersionNumber = x.Header!.EntryVersionNumber!.Value + 1;
        });
    }
    public ClearanceRequestBuilder<T> WithArrivalDateTimeOffset(DateOnly? date, TimeOnly? time,
        int maxHoursOffset = 12, int maxMinsOffset = 30)
    {
        var d = date ?? DateTime.Today.ToDate();
        var t = time ?? DateTime.Now.ToTime();
        var hoursOffset = CreateRandomInt(maxHoursOffset * -1, maxHoursOffset);
        var minsOffset = CreateRandomInt(maxMinsOffset * -1, maxMinsOffset);

        var dateTime = d.ToDateTime(t)
            .AddHours(hoursOffset)
            .AddMinutes(minsOffset);

        return Do(x => x.Header!.ArrivalDateTime = dateTime);
    }

    public ClearanceRequestBuilder<T> WithItem(string documentCode, string commodityCode, string description,
        int netWeight, string[]? checks = null)
    {
        checks = checks ?? ["H2019"];

        return Do(cr =>
        {
            cr.Items![0].TaricCommodityCode = commodityCode;
            cr.Items![0].GoodsDescription = description;
            cr.Items![0].ItemNetMass = netWeight;
            cr.Items![0].Documents![0].DocumentCode = documentCode;

            cr.Items![0].Checks = checks
                .Select(c => new Check() { CheckCode = c })
                .ToArray();
        });
    }

    public ClearanceRequestBuilder<T> WithItemDocumentRef(string documentRef, int itemNumber = 0, int documentNumber = 0)
    {
        return Do(cr =>
        {
            cr.Items![itemNumber].Documents![documentNumber].DocumentReference = documentRef;
        });
    }

    // public ClearanceRequestBuilder<T> WithMultipleIDocumentItems(string documentCode, string commodityCode, string description,
    //     int netWeight, string[] documentRefs, string checkCode = "H2019")
    // {
    //     return Do(cr =>
    //     {
    //         cr.Items![0].TaricCommodityCode = commodityCode;
    //         cr.Items![0].GoodsDescription = description;
    //         cr.Items![0].ItemNetMass = netWeight;
    //         cr.Items![0].Documents![0].DocumentCode = documentCode;
    //         cr.Items![0].Documents![0].DocumentReference = documentRefs[0];
    //         cr.Items![0].Checks![0].CheckCode = checkCode;
    //         
    //     });
    // }

    public ClearanceRequestBuilder<T> WithItemNoChecks(string documentCode, string commodityCode, string description,
        int netWeight)
    {
        return Do(cr =>
        {
            cr.Items![0].TaricCommodityCode = commodityCode;
            cr.Items![0].GoodsDescription = description;
            cr.Items![0].ItemNetMass = netWeight;
            cr.Items![0].Documents![0].DocumentCode = documentCode;
            cr.Items![0].Checks = [];
        });
    }

    public ClearanceRequestBuilder<T> WithRandomItems(int min, int max)
    {
        var commodityCount = CreateRandomInt(min, max);

        return Do(cr =>
        {
            var items = Enumerable.Range(0, commodityCount)
                .Select(_ => cr.Items![0])
                .ToArray();

            cr.Items = items;
        });
    }

    public ClearanceRequestBuilder<T> WithValidDocumentReferenceNumbers()
    {
        return Do(x =>
        {
            foreach (var item in x.Items!)
            {
                foreach (var document in item.Documents!)
                {
                    document.DocumentReference = "GBCHD2024.1001278";
                    document.DocumentCode = "C640";
                }
            }
        });
    }

    protected override ClearanceRequestBuilder<T> Validate()
    {
        return Do(cr =>
        {
            cr.Header!.EntryReference.AssertHasValue("Clearance Request EntryReference missing");
            cr.Header!.EntryVersionNumber.AssertHasValue("Clearance Request EntryVersionNumber missing");
            cr.Header!.DeclarationUcr.AssertHasValue("Clearance Request DeclarationUcr missing");

            // masterUcr can be null
            // cr.Header!.MasterUcr.AssertHasValue("Clearance Request MasterUcr missing"); 

            // cr.Header!.ArrivalDateTime.AssertHasValue("Clearance Request ArrivalDateTime missing");

            Array.ForEach(cr.Items!, i => Array.ForEach(i.Documents!, d => d.DocumentReference.AssertHasValue()));
        });
    }
}