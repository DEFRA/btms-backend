namespace Btms.Analytics.Tests.Helpers;

public static class TestAssertionExtensions
{
    public static MultiSeriesDatasetAssertions Should(this List<Series>? instance)
    {
        return new MultiSeriesDatasetAssertions(instance);
    }
    public static SingleSeriesDatasetAssertions Should(this SingeSeriesDataset? instance)
    {
        return new SingleSeriesDatasetAssertions(instance);
    }
}