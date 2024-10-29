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

    public static Lens<AlvsClearanceRequest, Items[]> ItemsLens =
        new Lens<AlvsClearanceRequest, Items[]>(x => x.Items!);

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
        // Trying to figure out if there's a way to use lenses to allow the below to work! 
        // https://stackoverflow.com/questions/30938972/how-to-build-nested-property-with-autofixture
        return With(x => x.Items[0].Documents[0].DocumentReference, chedReference);

        // return With(x => x.Compose(ItemsLens));
    }
}