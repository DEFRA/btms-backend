using FluentAssertions;

namespace Btms.Analytics.Tests.Helpers;

public static class TestAssertionExtensions
{
    public static MultiSeriesDatasetAssertions Should(this List<Series<ByNumericDimensionResult>>? instance)
    {
        return new MultiSeriesDatasetAssertions(instance);
    }
    public static SingleSeriesDatasetAssertions Should(this SingleSeriesDataset? instance)
    {
        return new SingleSeriesDatasetAssertions(instance);
    }
    
    public static void ShouldBeCorrect(this List<DatetimeSeries> result)
    {
        result.Count.Should().Be(4);
        
        result.Select(r => r.Name)
            .Should().BeEquivalentTo("Investigate", "Linked", "Not Linked", "Error");
    }
    
    public static void ShouldBeCorrect(this List<DatetimeSeries> result, DateTime from, DateTime to,
        AggregationPeriod aggregationPeriod = AggregationPeriod.Day)
    {
        result.ShouldBeCorrect();

        var periodTimespan = to - from;
        var periods = (int)(aggregationPeriod == AggregationPeriod.Hour ? periodTimespan.TotalHours : periodTimespan.TotalDays);
        
        result.Should().AllSatisfy(r =>
        {
            r.Periods.Should().AllSatisfy(p =>
            {
                p.Period.Should().BeOnOrAfter(from);
                p.Period.Should().BeOnOrBefore(to);
            });
            r.Periods.Count.Should().Be(periods);
        });
    }
    
    
    public static void ShouldBeCorrect(this List<Series<ByNumericDimensionResult>> result)
    {
        
        result.Count.Should().Be(4);
        result.Select(r => r.Name).Should().BeEquivalentTo("Investigate", "Linked", "Not Linked", "Error");
        
        result.Should().AllSatisfy(r =>
        {
            r.Dimension.Should().Be("Item Count");
            r.Results.Count.Should().NotBe(0);
        });
        
        result.Should().HaveResults();

        result.Should().BeSameLength();
    }

    public static void ShouldBeCorrect(this SingleSeriesDataset result)
    {
        result.Values.Count.Should().Be(4);
        result.Values.Keys.Should().BeEquivalentTo("Investigate", "Linked", "Not Linked", "Error");
    }
    
    public static void ShouldBeCorrect(this MultiSeriesDataset<ByNumericDimensionResult> result)
    {
        result.Series!.Count.Should().Be(4);
        result.Series.Should().HaveResults();
        result.Series.Should().BeSameLength();
            
        result.Series!.Should().AllSatisfy(d =>
        {
            d.Dimension.Should().Be("Document Reference Count");
        });
    }
    
    
}