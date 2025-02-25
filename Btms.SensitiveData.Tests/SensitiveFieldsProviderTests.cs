using FluentAssertions;
using Xunit;

namespace Btms.SensitiveData.Tests;

public class SensitiveFieldsProviderTests
{
    record ImportNotification(int Id);

    record NoFieldList(int Id);

    [Fact]
    public void WhenTypeHasList_ShouldReturnList()
    {
        // ARRANGE
        SensitiveFieldsProvider provider = new SensitiveFieldsProvider();

        // ACT
        var list = provider.Get(typeof(ImportNotification));

        // ASSERT
        list.Count.Should().BeGreaterThan(1);
    }

    [Fact]
    public void WhenTypeNotHaveList_ShouldReturnEmptyList()
    {
        // ARRANGE
        SensitiveFieldsProvider provider = new SensitiveFieldsProvider();

        // ACT
        var list = provider.Get(typeof(NoFieldList));

        // ASSERT
        list.Count.Should().Be(0);
    }
}