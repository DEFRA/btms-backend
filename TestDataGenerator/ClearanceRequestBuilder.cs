using System.Text.Json;
using Btms.Common.Extensions;
using Btms.Model;
using Btms.Types.Alvs;
using TestDataGenerator.Helpers;

namespace TestDataGenerator;

public class ClearanceRequestBuilder(string file) : ClearanceRequestBuilder<AlvsClearanceRequest>(file);
// {
//     public DateTime Created => base.Created;
//
//     public string Id => base.Id;
// }

public class ClearanceRequestBuilder<T> : BuilderBase<T, ClearanceRequestBuilder<T>>
    where T : AlvsClearanceRequest, new()
{
    private ClearanceRequestBuilder(): base(GetInitialValues)
    {
    }

    protected ClearanceRequestBuilder(string? file = null, string? itemJson = null) : base(GetInitialValues, file, itemJson)
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
        
        var builder =  new ClearanceRequestBuilder<T>(itemJson: json);
        
        return builder;
    }

    public ClearanceRequestBuilder<T> WithReferenceNumber(string chedReference)
    {
        var id = MatchIdentifier.FromNotification(chedReference);
        var clearanceRequestDocumentReference = id.AsCdsDocumentReference();

        base.Id = id.AsCdsEntryReference();
        
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

    public ClearanceRequestBuilder<T> WithCreationDate(DateTime entryDate, bool randomTime = true)
    {
        var entry = randomTime ?
            // We don't want documents created in the future!
            entryDate.RandomTime(entryDate.Date == DateTime.Today ? DateTime.Now.AddHours(-2).Hour : 23)
            : entryDate;

        base.Created = entry;
        return Do(x => x.ServiceHeader!.ServiceCallTimestamp = entry);
    }
    public ClearanceRequestBuilder<T> WithEntryVersionNumber(int version)
    {
        return Do(x => x.Header!.EntryVersionNumber = version);
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
        int netWeight, string checkCode = "H2019")
    {
        return Do(cr =>
        {
            cr.Items![0].TaricCommodityCode = commodityCode;
            cr.Items![0].GoodsDescription = description;
            cr.Items![0].ItemNetMass = netWeight;
            cr.Items![0].Documents![0].DocumentCode = documentCode;
            cr.Items![0].Checks![0].CheckCode = checkCode;
        });
    }
    
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
            cr.Header!.DeclarationUcr.AssertHasValue("Clearance Request DeclarationUcr missing");
            cr.Header!.MasterUcr.AssertHasValue("Clearance Request MasterUcr missing");
            // cr.Header!.ArrivalDateTime.AssertHasValue("Clearance Request ArrivalDateTime missing");

            Array.ForEach(cr.Items!, i => Array.ForEach(i.Documents!, d => d.DocumentReference.AssertHasValue()));
        });
    }

    private static (DateTime? created, string? id) GetInitialValues(T message)
    {
        // var cr = (AlvsClearanceRequest)message;
        return (message.ServiceHeader?.ServiceCallTimestamp, message.Header?.EntryReference);
        // throw new NotImplementedException();
    }
}