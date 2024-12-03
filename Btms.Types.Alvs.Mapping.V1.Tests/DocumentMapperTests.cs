using AutoBogus;
using Btms.Types.Alvs;
using Btms.Types.Alvs.Mapping;
using FluentAssertions;
using Xunit;

namespace Btms.Types.Alvs.Mapping.V1.Tests;

public class DocumentMapperTests
{
    [Fact]
    public void SimpleMapperTest()
    {
        var faker = AutoFaker.Create();
        var sourceValue = faker.Generate<Document>();

        var mappedValue = DocumentMapper.Map(sourceValue);


        mappedValue.DocumentReference.Should().Be(sourceValue.DocumentReference);
        mappedValue.DocumentCode.Should().Be(sourceValue.DocumentCode);
        mappedValue.DocumentControl.Should().Be(sourceValue.DocumentControl);
        mappedValue.DocumentQuantity.Should().Be(sourceValue.DocumentQuantity);
        mappedValue.DocumentStatus.Should().Be(sourceValue.DocumentStatus);
    }
}