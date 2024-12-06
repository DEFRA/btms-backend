namespace Btms.Analytics;

public class ByDateTimeResult
{
    public DateTime Period { get; set; }
    public int Value { get; set; }
}

public class ByNumericDimensionResult
{
    public int Dimension { get; set; }
    public int Value { get; set; }
}

public class SingeSeriesDataset
{
    public IDictionary<string, int> Values { get; set; } = new Dictionary<string, int>();
}

public class MultiSeriesDatetimeDataset(string name)
{
    public string Name { get; set; } = name;
    public List<ByDateTimeResult> Periods { get; set; } = [];
}

public class MultiSeriesDataset(string name, string dimension)
{
    public string Name { get; set; } = name;
    public string Dimension { get; set; } = dimension;
    public List<ByNumericDimensionResult> Results { get; set; } = [];
}