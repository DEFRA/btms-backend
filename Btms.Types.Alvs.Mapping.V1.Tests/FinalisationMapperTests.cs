using AutoBogus;
using Btms.Model.Cds;
using FluentAssertions;
using Xunit;

namespace Btms.Types.Alvs.Mapping.V1.Tests;

public class FinalisationMapperTests
{

    private Finalisation GetFake(int finalState = 1, string manualAction = "Y")
    {
        var faker = AutoFaker.Create();
        var sourceValue = faker.Generate<Finalisation>();
        sourceValue.Header.FinalState = finalState.ToString();
        sourceValue.Header.ManualAction = manualAction;

        return sourceValue;
    }

    [Fact]
    public void FinalState_ShouldBeCancelled()
    {
        var sourceValue = GetFake();

        var mappedValue = FinalisationMapper.Map(sourceValue);

        mappedValue.Header.FinalState.Should().Be(FinalState.CancelledAfterArrival);
    }

    [Fact]
    public void FinalState_ShouldBeCleared()
    {
        var sourceValue = GetFake(finalState: 0);

        var mappedValue = FinalisationMapper.Map(sourceValue);

        mappedValue.Header.FinalState.Should().Be(FinalState.Cleared);
    }

    [Fact]
    public void FinalState_ShouldBeDestroyed()
    {
        var sourceValue = GetFake(finalState: 3);

        var mappedValue = FinalisationMapper.Map(sourceValue);

        mappedValue.Header.FinalState.Should().Be(FinalState.Destroyed);
    }


    [Fact]
    public void ManualAction_ShouldBeTrue()
    {
        var sourceValue = GetFake();

        var mappedValue = FinalisationMapper.Map(sourceValue);

        mappedValue.Header.ManualAction.Should().BeTrue();
    }

    [Fact]
    public void ManualAction_ShouldBeFalse()
    {
        var sourceValue = GetFake(manualAction: "N");

        var mappedValue = FinalisationMapper.Map(sourceValue);

        mappedValue.Header.ManualAction.Should().BeFalse();
    }
}