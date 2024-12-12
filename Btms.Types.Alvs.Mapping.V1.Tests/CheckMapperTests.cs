using AutoBogus;
using FluentAssertions;
using Xunit;

namespace Btms.Types.Alvs.Mapping.V1.Tests;

public class CheckMapperTests
{
    [Fact]
    public void SimpleMapperTest()
    {
        var faker = AutoFaker.Create();
        var sourceValue = faker.Generate<Check>();

        var mappedValue = CheckMapper.Map(sourceValue);


        mappedValue.CheckCode.Should().Be(sourceValue.CheckCode);
        mappedValue.DepartmentCode.Should().Be(sourceValue.DepartmentCode);
    }
}