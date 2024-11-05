using System.Runtime.Serialization;
using Cdms.Model;
using Cdms.Types.Alvs;
using Cdms.Types.Ipaffs;

namespace TestDataGenerator;

public class ClearanceRequestBuilder : ClearanceRequestBuilder<AlvsClearanceRequest>
{
    public ClearanceRequestBuilder()
    {
    }

    public ClearanceRequestBuilder(string file) : base(file)
    {
    }
}

public class ClearanceRequestBuilder<T> : BuilderBase<T, ClearanceRequestBuilder<T>>
    where T : AlvsClearanceRequest, new()
{
    protected ClearanceRequestBuilder() : base()
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
        var clearanceRequestDocumentReference =
            MatchIdentifier.FromNotification(chedReference).AsCdsDocumentReference();
        
        return Do(x => 
            Array.ForEach(x.Items!, i => 
                Array.ForEach(i.Documents!, d=> d.DocumentReference = clearanceRequestDocumentReference)));
    }

    public ClearanceRequestBuilder<T> WithEntryDate(DateTime entryDate)
    {
        return Do(x => x.ServiceHeader!.ServiceCallTimestamp = entryDate);
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
}