using Btms.Business.Services.Decisions.Finders;
using Xunit;

namespace Btms.Business.Tests.Services.Decisions.Finders;

public class ChedDDecisionFinderTests
{
    [Fact]
    public async Task Find_Should_DoThings()
    {
        // Arrange
        var sut = new ChedDDecisionFinder();
        
        // Act

        // Assert
        await Task.CompletedTask;
    }
}