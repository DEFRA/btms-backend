namespace Btms.Backend.Cli;

public static class RootPaths
{
    private static readonly string RootFolder = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", ".."));
    public static readonly string BackendFolder = Path.GetFullPath(Path.Combine(RootFolder, "Btms.Backend"));
    public static readonly string BackendCliFolder = Path.GetFullPath(Path.Combine(RootFolder, "Btms.Backend.Cli"));
    public static readonly string ModelFolder = Path.GetFullPath(Path.Combine(RootFolder, "Btms.Model"));
    public static readonly string TypesPartialFolder = Path.GetFullPath(Path.Combine(RootFolder, "Btms.Types"));
}