using Btms.Business.Pipelines;
using FluentAssertions;
using MediatR;
using NSubstitute;
using Xunit;

namespace Btms.Business.Tests.Pipelines;

public class TerminatePipelineBaseTests
{
    [Fact]
    public async Task Handle_ValidRequest_ShouldReturnFalse()
    {
        // Arrange
        var mockNextDelegate = Substitute.For<RequestHandlerDelegate<PipelineResult>>();

        var sut = new PipelineTestHelpers.MockTerminatePipeline();
        var request = new PipelineTestHelpers.MockRequest(new PipelineTestHelpers.MockContext());

        // Act
        var result = await sut.Handle(request, mockNextDelegate, CancellationToken.None);

        // Assert
        result.ExitPipeline.Should().BeFalse();
        await mockNextDelegate.DidNotReceive().Invoke();
    }
}