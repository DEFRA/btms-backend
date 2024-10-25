using Cdms.Types.Ipaffs;
using FluentAssertions;
using Xunit;

namespace TestDataGenerator.Tests;

public class ImportNotificationBuilderTests
{
    [Fact]
    public void WithReferenceNumber_WithChedA_ShouldCreateCorrectReference()
    {
        var builder = ImportNotificationBuilder.Default();
        builder.WithReferenceNumber(ImportNotificationTypeEnum.Cveda, 1);
        var notification = builder.Build();
        notification.ReferenceNumber.Should().StartWith("CHEDA");
    }
    
    [Fact]
    public void WithReferenceNumber_WithChedD_ShouldCreateCorrectReference()
    {
        var builder = ImportNotificationBuilder.Default();
        builder.WithReferenceNumber(ImportNotificationTypeEnum.Ced, 1);
        var notification = builder.Build();
        notification.ReferenceNumber.Should().StartWith("CHEDD");
    }
    
    
    [Fact]
    public void WithRandomCommodities_5_ShouldHave5Commodities()
    {
        var builder = ImportNotificationBuilder.Default();
        builder.WithRandomCommodities(5, 5);
        var notification = builder.Build();
        notification.PartOne.Commodities.CommodityComplements.Length.Should().Be(5);
    }
}