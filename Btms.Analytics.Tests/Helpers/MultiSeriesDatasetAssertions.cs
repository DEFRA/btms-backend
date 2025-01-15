using FluentAssertions;
using FluentAssertions.Collections;

namespace Btms.Analytics.Tests.Helpers;

public class MultiSeriesDatasetAssertions(List<Series<ByNumericDimensionResult>>? test)
    : GenericCollectionAssertions<Series<ByNumericDimensionResult>>(test)
{
    [CustomAssertion]
    public void BeSameLength(string because = "", params object[] becauseArgs)
    {
        test!.Select(r => r.Results.Count)
            .Distinct()
            .Count()
            .Should()
            .Be(1);
    }
    
    [CustomAssertion]
    public void HaveResults(string because = "", params object[] becauseArgs)
    {
        test!.Sum(d => d.Results.Sum(r => ((ByNumericDimensionResult)r).Value))
            .Should().BeGreaterThan(0);
    }
}