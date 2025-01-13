using System.Reflection;
using Btms.Common.Extensions;
using Xunit;
using JetBrains.Annotations;

namespace TestGenerator.IntegrationTesting.Backend;

[IgnoreXunitAnalyzersRule1013]
public class FailingFactAttribute: FactAttribute
{
    public FailingFactAttribute(string? jiraTicket = null, string? reason = null)
    {
        if (!jiraTicket.HasValue() && !reason.HasValue())
        {
            throw new CustomAttributeFormatException("Need a JIRA ticket and/or reason");
        }

        if (jiraTicket.HasValue())
        {
            base.Skip = "Skipping. Logged as JIRA ticket " + jiraTicket; 
        }
        else if (reason.HasValue())
        {
            base.Skip = reason;
        }
    }
}