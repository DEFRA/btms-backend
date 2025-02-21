using Btms.Common.Enum;
using Btms.Model;
using Btms.Model.Cds;
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

    private static void ShouldBeEquivalentToLinkStatusEnum(this List<string> result)
    {
        var enumLookup = new JsonStringEnumConverterEx<LinkStatusEnum>();

        var fields = enumLookup.GetValues();
        result.Count().Should().Be(fields.Count());

        result
            .Should().BeEquivalentTo(fields);
    }

    public static void ShouldBeCorrectBasedOnLinkStatusEnum(this List<DatetimeSeries> result)
    {
        result.Select(r => r.Name)
            .ToList()
            .ShouldBeEquivalentToLinkStatusEnum();
    }

    public static void ShouldBeCorrectBasedOnLinkStatusEnum(this List<DatetimeSeries> result, DateTime from, DateTime to,
        AggregationPeriod aggregationPeriod = AggregationPeriod.Day)
    {
        result.ShouldBeCorrectBasedOnLinkStatusEnum();

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


    public static void ShouldBeCorrectBasedOnLinkStatusEnum(this List<Series<ByNumericDimensionResult>> result)
    {

        result.Select(r => r.Name)
            .ToList()
            .ShouldBeEquivalentToLinkStatusEnum();

        result.Should().AllSatisfy(r =>
        {
            r.Dimension.Should().Be("Item Count");
            r.Results.Count.Should().NotBe(0);
        });

        result.Should().HaveResults();

        result.Should().BeSameLength();
    }

    public static void ShouldBeCorrectBasedOnLinkStatusEnum(this SingleSeriesDataset result)
    {
        result.Values.Keys
            .ToList()
            .ShouldBeEquivalentToLinkStatusEnum();
    }

    public static void ShouldBeCorrectBasedOnLinkStatusEnum(this MultiSeriesDataset<ByNumericDimensionResult> result)
    {
        result.Series.Select(s => s.Name)
            .ToList()
            .ShouldBeEquivalentToLinkStatusEnum();

        // result.Series!.Count.Should().Be(4);
        result.Series.Should().HaveResults();
        result.Series.Should().BeSameLength();

        result.Series!.Should().AllSatisfy(d =>
        {
            d.Dimension.Should().Be("Document Reference Count");
        });
    }


}