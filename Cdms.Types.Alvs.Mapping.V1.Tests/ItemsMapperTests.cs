using AutoBogus;
using FluentAssertions;
using Xunit;

namespace Cdms.Types.Alvs.Mapping.V1.Tests;

public class ItemsMapperTests
{
    [Fact]
    public void SimpleMapperTest()
    {
        var faker = AutoFaker.Create();
        var sourceValue = faker.Generate<Types.Alvs.Items>();

        var mappedValue = ItemsMapper.Map(sourceValue);


        mappedValue.ItemNumber.Should().Be(sourceValue.ItemNumber);
        mappedValue.CustomsProcedureCode.Should().Be(sourceValue.CustomsProcedureCode);
        mappedValue.TaricCommodityCode.Should().Be(sourceValue.TaricCommodityCode);
        mappedValue.GoodsDescription.Should().Be(sourceValue.GoodsDescription);
        mappedValue.ConsigneeId.Should().Be(sourceValue.ConsigneeId);
        mappedValue.ConsigneeName.Should().Be(sourceValue.ConsigneeName);
        mappedValue.ItemNetMass.Should().Be(sourceValue.ItemNetMass);
        mappedValue.ItemSupplementaryUnits.Should().Be(sourceValue.ItemSupplementaryUnits);
        mappedValue.ItemThirdQuantity.Should().Be(sourceValue.ItemThirdQuantity);
        mappedValue.ItemOriginCountryCode.Should().Be(sourceValue.ItemOriginCountryCode);
        if (sourceValue.Documents != null) mappedValue.Documents?.Length.Should().Be(sourceValue.Documents.Length);
        if (sourceValue.Checks != null) mappedValue.Checks?.Length.Should().Be(sourceValue.Checks.Length);
    }
}