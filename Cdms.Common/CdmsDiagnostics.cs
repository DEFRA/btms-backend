using System.Diagnostics;

namespace Cdms.Common;

public static class CdmsDiagnostics
{
    public static readonly string ActivitySourceName = "Cdms";
    public static readonly ActivitySource ActivitySource = new(ActivitySourceName);
}