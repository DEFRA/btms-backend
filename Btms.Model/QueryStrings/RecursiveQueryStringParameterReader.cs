using Humanizer;
using JsonApiDotNetCore.Controllers.Annotations;
using JsonApiDotNetCore.QueryStrings;
using Microsoft.Extensions.Primitives;

namespace Btms.Model.QueryStrings;

public class RecursiveQueryStringParameterReader : IQueryStringParameterReader
{
    private const string ParameterName = "recursive:include";
    
    public bool IsEnabled(DisableQueryStringAttribute disableQueryStringAttribute)
    {
        return true;
    }

    public bool CanRead(string parameterName)
    {
        return ParameterName.Equals(parameterName, StringComparison.OrdinalIgnoreCase);
    }

    public void Read(string parameterName, StringValues parameterValue)
    {
        if (ParameterName.Equals(parameterName, StringComparison.OrdinalIgnoreCase))
        {
            
        }
    }

    public bool AllowEmptyValue { get; } = true;
}