using System.Reflection;
using Btms.Common.Extensions;
using Xunit;
using JetBrains.Annotations;
using Xunit.Sdk;

namespace TestGenerator.IntegrationTesting.Backend;

// Should be used as:
// [FailingFact(jiraTicket:"CDMS-234"), Trait("JiraTicket", "CDMS-234")]
// unfortunately we can't combine the trait into a single fact attribute atm as it's not picked
// up correctly. Seems to be an issue with TraitDiscoverer but needs some further investigation
// filtering & categorisation only works with the built in Trait and not
// custom implementations at present:
// https://youtrack.jetbrains.com/issue/RIDER-26346/xUnit-categories-making-use-of-ITraitAttribute-and-ITraitDiscoverer-doesnt-seem-to-be-discovered-by-Riders-category-function.
// https://youtrack.jetbrains.com/issue/RIDER-115680/xUnit-categories-making-use-of-ITraitAttribute-and-ITraitDiscoverer-doesnt-seem-to-be-discovered-by-Riders-category-function.

[IgnoreXunitAnalyzersRule1013]
public class FailingFactAttribute: FactAttribute
{
    // private static readonly string[] IncompleteJiraTickets = ["CDMS-234", "CDMS-235", "CDMS-232", "CDMS-205"];
    private static readonly string[] IncompleteJiraTickets = ["CDMS-235", "CDMS-232", "CDMS-205"];
    // private static readonly string[] IncompleteJiraTickets = [];

    public FailingFactAttribute(string? jiraTicket = null, string? reason = null)
    {
        if (!jiraTicket.HasValue() && !reason.HasValue())
        {
            throw new CustomAttributeFormatException("Need a JIRA ticket and/or reason");
        }

        if (jiraTicket.HasValue() && !IncompleteJiraTickets.Contains(jiraTicket)) return;

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

// /// <summary>
// /// Apply this attribute to your test method to specify a category.
// /// </summary>
// [AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
// // [TraitDiscoverer("Xunit.Sdk.TraitDiscoverer", "xunit.core")]
// public class CategoryAttribute(string category) : Attribute, ITraitAttribute
// {
//     // Note that one trait attribute can provide as many traits as it needs to; you're not limited
//     // to just one trait from one attribute.
//     public IReadOnlyCollection<KeyValuePair<string, string>> GetTraits() =>
//     [
//         new("Category", category),
//         new("Categorized", "true"),
//     ];
// }

// [XunitTestCaseDiscoverer("Xunit.Sdk.FactDiscoverer", "xunit.execution.{Platform}")]
// [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
// public class FactAttribute : Attribute
// {
//     public virtual string DisplayName { get; set; }
//     public virtual string Skip { get; set; }
//     public virtual int Timeout { get; set; }
// }
//
// [TraitDiscoverer("Xunit.Sdk.TraitDiscoverer", "xunit.core")]
// [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = true)]
// public sealed class TraitAttribute : Attribute, ITraitAttribute
// {
//     public TraitAttribute(string name, string value) { }
// }

// [XunitTestCaseDiscoverer(typeof(Xunit.Sdk.FactDiscoverer))]
// [XunitTestCaseDiscoverer("Xunit.Sdk.FactDiscoverer", "xunit.execution.{Platform}"),
//  TraitDiscoverer("Xunit.Sdk.TraitDiscoverer", "xunit.core")]
// public class FactTraitAttribute(string name, string value) : FactAttribute, ITraitAttribute
// {
// }
//
// public class FactIntegrationAttribute : FactTraitAttribute
// {
//     public FactIntegrationAttribute() : base("JiraTicket", "CDMS-234") { }
// }

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
