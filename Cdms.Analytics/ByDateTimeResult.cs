namespace Cdms.Analytics;

public class ByDateTimeResult
{
    public DateTime Period { get; set; }
    // public Dictionary<string, string> BucketVariables { get; set; } = [];
    public int Value { get; set; }
}

public class PieChartDataset
{
    public IDictionary<string, int> Values { get; set; } = new Dictionary<string, int>();
}

public class Dataset(string name)
{
    public string Name { get; set; } = name;
    public List<ByDateTimeResult> Periods { get; set; } = [];
    
}