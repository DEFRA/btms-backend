using AutoBogus;
using FluentAssertions;
using Xunit;

namespace Btms.Types.Alvs.Mapping.V1.Tests;

public class HeaderMapperTests
{
    [Fact]
    public void SimpleMapperTest()
    {
        var faker = AutoFaker.Create();
        var sourceHeader = faker.Generate<Header>();

        var mappedHeader = HeaderMapper.Map(sourceHeader);

        mappedHeader!.EntryReference.Should().Be(sourceHeader.EntryReference);
        mappedHeader.EntryVersionNumber.Should().Be(sourceHeader.EntryVersionNumber);
        mappedHeader.PreviousVersionNumber.Should().Be(sourceHeader.PreviousVersionNumber);
        mappedHeader.DeclarationUcr.Should().Be(sourceHeader.DeclarationUcr);
        mappedHeader.DeclarationPartNumber.Should().Be(sourceHeader.DeclarationPartNumber);
        mappedHeader.DeclarationType.Should().Be(sourceHeader.DeclarationType);
        mappedHeader.ArrivesAt.Should().Be(sourceHeader.ArrivalDateTime);
        mappedHeader.SubmitterTurn.Should().Be(sourceHeader.SubmitterTurn);
        mappedHeader.DeclarantId.Should().Be(sourceHeader.DeclarantId);

        mappedHeader.DeclarantName.Should().Be(sourceHeader.DeclarantName);
        mappedHeader.DispatchCountryCode.Should().Be(sourceHeader.DispatchCountryCode);
        mappedHeader.GoodsLocationCode.Should().Be(sourceHeader.GoodsLocationCode);
        mappedHeader.MasterUcr.Should().Be(sourceHeader.MasterUcr);
    }
}