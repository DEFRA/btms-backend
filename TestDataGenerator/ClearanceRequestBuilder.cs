using Cdms.Common.Extensions;
using Cdms.Model;
using Cdms.Types.Alvs;
using TestDataGenerator.Helpers;

namespace TestDataGenerator;

public class ClearanceRequestBuilder(string file) : ClearanceRequestBuilder<AlvsClearanceRequest>(file);

public class ClearanceRequestBuilder<T> : BuilderBase<T, ClearanceRequestBuilder<T>>
    where T : AlvsClearanceRequest, new()
{
    private ClearanceRequestBuilder() : base()
    {
    }

    protected ClearanceRequestBuilder(string file) : base(file)
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

    public ClearanceRequestBuilder<T> WithReferenceNumber(string chedReference)
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

    public ClearanceRequestBuilder<T> WithEntryDate(DateTime entryDate)
    {
        return Do(x => x.ServiceHeader!.ServiceCallTimestamp = entryDate.RandomTime());
    }

    public ClearanceRequestBuilder<T> WithArrivalDateTimeOffset(DateOnly? date, TimeOnly? time, 
        int maxHoursOffset = 12, int maxMinsOffset = 30)
    {
        DateOnly d = date ?? DateTime.Today.ToDate();
        TimeOnly t = time ?? DateTime.Now.ToTime();
        var hoursOffset = CreateRandomInt(maxHoursOffset * -1, maxHoursOffset);
        var minsOffset = CreateRandomInt(maxMinsOffset * -1, maxMinsOffset);

        var dateTime = d.ToDateTime(t)
            .AddHours(hoursOffset)
            .AddMinutes(minsOffset);
        
        return Do(x => x.Header!.ArrivalDateTime = dateTime);
    }

    public ClearanceRequestBuilder<T> WithItem(string documentCode, string commodityCode, string description,
        int netWeight)
    {
        return Do(x =>
        {
            x.Items![0].TaricCommodityCode = commodityCode;
            x.Items![0].GoodsDescription = description;
            x.Items![0].ItemNetMass = netWeight;
            x.Items![0].Documents![0].DocumentCode = documentCode;
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
            cr.Header!.ArrivalDateTime.AssertHasValue("Clearance Request ArrivalDateTime missing");

            Array.ForEach(cr.Items!, i => Array.ForEach(i.Documents!, d => d.DocumentReference.AssertHasValue()));
        });
    }
}