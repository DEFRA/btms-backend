namespace Btms.Analytics.Extensions;

public static class SummarisedDatasetExtensions
{
    public static async Task<IDataset> AsIDataset<TSummary,TResult>(this Task<SummarisedDataset<TSummary, TResult>> ms)
        where TResult : IDimensionResult
        where TSummary : IDimensionResult
    {
        await ms;
        return (IDataset)ms.Result;
    }
}