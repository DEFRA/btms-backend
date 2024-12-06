using Btms.Common.Extensions;
using FluentAssertions;
using FluentAssertions.Collections;

namespace Btms.Analytics.Tests.Helpers;

public class SingleSeriesDatasetAssertions(SingeSeriesDataset? test)
{   
    [CustomAssertion]
    public void HaveResults(string because = "", params object[] becauseArgs)
    {
        test!.Values
            .Values.Sum()
            .Should().BeGreaterThan(0);
    }
}