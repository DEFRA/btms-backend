using Btms.Business.Pipelines;
using Btms.Business.Pipelines.Matching;
using FluentAssertions;
using Xunit;

namespace Btms.Business.Tests.Pipelines.Matching;

public class MatchPostProcessTests
{
    [Fact]
    public async Task Process_ValidRequest_RecordsPostProcessing()
    {
        // Arrange
        var sut = new MatchPostProcess();
        var request = new MatchRequest(new MatchContext());
        var result = new PipelineResult(false);

        var expectedRecord = "Did Post Processing";

        // Act
        await sut.Process(request, result, CancellationToken.None);

        // Assert
        request.Context.Record.Should().StartWith(expectedRecord);
    }
}