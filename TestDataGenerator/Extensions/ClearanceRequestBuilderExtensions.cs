using Btms.Types.Alvs;

namespace TestDataGenerator.Extensions;

public static class ClearanceRequestBuilderExtensions
{
    public static ClearanceRequestBuilder<AlvsClearanceRequest> WithTunaItem(this ClearanceRequestBuilder<AlvsClearanceRequest> builder)
    {
        return builder
            .WithItem("N853", "16041421", "Tuna ROW CHEDP", 900, checks: ["H222", "H224"]);
    }
}