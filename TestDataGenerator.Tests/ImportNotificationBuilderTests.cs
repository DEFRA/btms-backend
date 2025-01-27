using Btms.Types.Ipaffs;
using Btms.Common.Extensions;
using FluentAssertions;
using Xunit;

namespace TestDataGenerator.Tests;

public class ImportNotificationBuilderTests
{
    [Fact]
    public void WithReferenceNumber_WithChedA_ShouldCreateCorrectReference()
    {
        var builder = ImportNotificationBuilder.Default();
        builder.WithReferenceNumber(ImportNotificationTypeEnum.Cveda, 1, DateTime.Now, 1);
        var notification = builder.Build();
        notification.ReferenceNumber.Should().StartWith("CHEDA");
    }

    [Fact]
    public void WithReferenceNumber_WithChedD_ShouldCreateCorrectReference()
    {
        var builder = ImportNotificationBuilder.Default();
        builder.WithReferenceNumber(ImportNotificationTypeEnum.Ced, 1, DateTime.Now, 1);
        var notification = builder.Build();
        notification.ReferenceNumber.Should().StartWith("CHEDD");
    }

    [Fact]
    public void WithRandomCommodities_5_ShouldHave5Commodities()
    {
        var builder = ImportNotificationBuilder.Default();
        builder.WithRandomCommodities(5, 5);
        var notification = builder.Build();
        notification.PartOne?.Commodities?.CommodityComplements?.Length.Should().Be(5);
    }

    [Fact]
    public void WithEntryDate_ShouldSet()
    {
        var date = DateTime.Today.AddDays(-5);
        var builder = ImportNotificationBuilder.Default();
        builder.WithCreationDate(date);

        var notification = builder.Build();
        notification.LastUpdated.ToDate().Should().Be(date.ToDate());
    }
    
    [Fact]
    public void Creating_Multiple_Objects_ShouldNotUpdateSecond()
    {
        var date = DateTime.Today.AddDays(-5);
        var builder = ImportNotificationBuilder.Default();
        builder.WithCreationDate(date);
        
        var before = builder.ValidateAndBuild();
        
        var after = builder
            .Clone()
            .WithCreationDate(date.AddDays(1))
            .ValidateAndBuild();

        var notification = builder.Build();
        before.LastUpdated.Should().NotBe(after.LastUpdated);
    }
}