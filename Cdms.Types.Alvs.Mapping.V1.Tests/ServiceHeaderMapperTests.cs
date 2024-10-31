using AutoBogus;
using FluentAssertions;
using Xunit;

namespace Cdms.Types.Alvs.Mapping.V1.Tests;

public class ServiceHeaderMapperTests
{
    [Fact]
    public void SimpleMapperTest()
    {
        var faker = AutoFaker.Create();
        var sourceValue = faker.Generate<Types.Alvs.ServiceHeader>();

        var mappedValue = ServiceHeaderMapper.Map(sourceValue);


        mappedValue.ServiceCalled.Should().Be(sourceValue.ServiceCallTimestamp);
        mappedValue.CorrelationId.Should().Be(sourceValue.CorrelationId);
        mappedValue.DestinationSystem.Should().Be(sourceValue.DestinationSystem);
        mappedValue.SourceSystem.Should().Be(sourceValue.SourceSystem);
    }
}