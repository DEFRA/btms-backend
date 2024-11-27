namespace Cdms.Analytics;

public class ByDateResult
{
    public DateOnly Date { get; set; }
    public Dictionary<string, string> BucketVariables { get; set; } = [];
    public int Value { get; set; }
}

public class Dataset(string name)
{
    public string Name { get; set; } = name;
    public List<ByDateResult> Dates { get; set; } = [];
    
}