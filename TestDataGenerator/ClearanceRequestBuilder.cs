using System.Runtime.Serialization;
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
    public ClearanceRequestBuilder() : base()
    {
    }

    // public static Lens<AlvsClearanceRequest, Items[]> ItemsLens =
    //     new Lens<AlvsClearanceRequest, Items[]>(x => x.Items!);

    public ClearanceRequestBuilder(string file) : base(file)
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

    public ClearanceRequestBuilder<T> WithFirstReferenceNumber(string chedReference)
    {
        // TODO : manipulate the ref to be correct format first
        return Do(x => x.Items![0].Documents![0].DocumentReference = chedReference);
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