using System.Reflection;
using Btms.Common.Extensions;
using Xunit;
using JetBrains.Annotations;
using Xunit.Sdk;

namespace TestGenerator.IntegrationTesting.Backend;

// [FailingFact(jiraTicket:"CDMS-234"), Trait("JiraTicket", "CDMS-234")]
// unfortunately we can't combine them into a single attribute as rider
// filtering & categorisation only works with the built in Trait and not
// custom implementations at present:
// https://youtrack.jetbrains.com/issue/RIDER-26346/xUnit-categories-making-use-of-ITraitAttribute-and-ITraitDiscoverer-doesnt-seem-to-be-discovered-by-Riders-category-function.
// https://youtrack.jetbrains.com/issue/RIDER-115680/xUnit-categories-making-use-of-ITraitAttribute-and-ITraitDiscoverer-doesnt-seem-to-be-discovered-by-Riders-category-function.

[IgnoreXunitAnalyzersRule1013]
public class FailingFactAttribute: FactAttribute, ITraitAttribute
{

    //Use with:
    public FailingFactAttribute(string? jiraTicket = null, string? reason = null)
    {
        if (!jiraTicket.HasValue() && !reason.HasValue())
        {
            throw new CustomAttributeFormatException("Need a JIRA ticket and/or reason");
        }

        if (jiraTicket.HasValue() && jiraTicket != "CDMS-234")
        {
            base.Skip = "Skipping. Logged as JIRA ticket " + jiraTicket; 
            // base.
        }
        else if (reason.HasValue())
        {
            base.Skip = reason;
        }
    }
}

/// <summary>
/// Apply this attribute to your test method to specify a category.
/// </summary>
[AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
[TraitDiscoverer("Xunit.Sdk.TraitDiscoverer", "xunit.core")]
public class CategoryAttribute(string category) : Attribute, ITraitAttribute
{
    // Note that one trait attribute can provide as many traits as it needs to; you're not limited
    // to just one trait from one attribute.
    public IReadOnlyCollection<KeyValuePair<string, string>> GetTraits() =>
    [
        new("Category", category),
        new("Categorized", "true"),
    ];
}
//
// [XunitTestCaseDiscoverer("Xunit.Sdk.FactDiscoverer", "xunit.execution.{Platform}")]
// [TraitDiscoverer("Xunit.Sdk.TraitDiscoverer", "xunit.core")]
// public class FailingFactTraitAttribute(string name, string value): FactAttribute, ITraitAttribute
// {
//     public string Name = name;
//     public string Value = value;
//     //
//     // public FailingFactAttribute(string name, string value)
//     // {
//     //     Name = name;
//     //     Value = value;
//     // }
// }
