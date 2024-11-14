using Cdms.Model;
using Cdms.Types.Alvs;
using TestDataGenerator.Helpers;
using Cdms.Common.Extensions;

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
        var id = MatchIdentifier.FromNotification(chedReference);
        var clearanceRequestDocumentReference =
            id.AsCdsDocumentReference();
        
        return 
            Do(x => x.Header!.EntryReference = id.AsCdsEntryReference())
            .Do(x => x.Header!.DeclarationUcr = id.AsCdsDeclarationUcr()) // We may want to revisit this
            .Do(x => x.Header!.MasterUcr = id.AsCdsMasterUcr()) // We may want to revisit this
            .Do(x => 
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

    public override ClearanceRequestBuilder<T> Validate()
    {
        return Do(cr =>
        {
            cr.Header!.EntryReference.AssertHasValue();
            cr.Header!.DeclarationUcr.AssertHasValue();
            cr.Header!.MasterUcr.AssertHasValue();
            
            Array.ForEach(cr.Items!, i=> Array.ForEach(i.Documents!, d => d.DocumentReference.AssertHasValue()));
        });
    }
}