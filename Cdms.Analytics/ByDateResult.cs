namespace Cdms.Analytics;

public class ByDateResult
{
    public DateOnly Date { get; set; }
    public Dictionary<string, string> BucketVariables { get; set; } = [];
    public int Value { get; set; }
}