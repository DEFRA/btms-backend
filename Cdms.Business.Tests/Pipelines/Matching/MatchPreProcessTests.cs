using Cdms.Business.Pipelines.Matching;
using FluentAssertions;
using Xunit;

namespace Cdms.Business.Tests.Pipelines.Matching;

public class MatchPreProcessTests
{
    [Fact]
    public async Task Process_ValidRequest_RecordsPreProcessing()
    {
        // Arrange
        var sut = new MatchPreProcess();
        var request = new MatchRequest(new MatchContext()
        {
            MatchReference = "ABC123"
        });
        
        var expectedRecord = $"Did pre-processing with initial request [{request.Context.MatchReference}]";
        
        // Act
        await sut.Process(request, CancellationToken.None);
        
        // Assert
        request.Context.Record.Should().StartWith(expectedRecord);
    }
}