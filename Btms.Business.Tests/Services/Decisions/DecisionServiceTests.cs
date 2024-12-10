using Btms.Business.Services.Decisions;
using Xunit;

namespace Btms.Business.Tests.Services.Decisions;

public class DecisionServiceTests
{
    [Fact]
    public async Task DeriveDecision()
    {
        // Arrange
        var sut = new DecisionService();
        
        // Act

        // Assert
        await Task.CompletedTask;
    }
}