using Cdms.Business.Pipelines;
using FluentAssertions;
using MediatR;
using NSubstitute;
using Xunit;

namespace Cdms.Business.Tests.Pipelines;

public class PipelineBaseTests
{
    [Fact]
    public async Task Handle_SuccessfullyCompletedMatch_ExitsPipeline()
    {
        // Arrange
        var mockNextDelegate = Substitute.For<RequestHandlerDelegate<PipelineResult>>();

        var stubService = Substitute.For<PipelineTestHelpers.IStubService>();
        stubService.ProcessFilter(Arg.Any<PipelineTestHelpers.MockContext>()).Returns(new PipelineResult(true));

        var sut = new PipelineTestHelpers.MockPipeline(stubService);
        var request = new PipelineTestHelpers.MockRequest(new PipelineTestHelpers.MockContext());

        // Act
        var result = await sut.Handle(request, mockNextDelegate, CancellationToken.None);

        // Assert
        result.ExitPipeline.Should().BeTrue();
        await mockNextDelegate.DidNotReceive().Invoke();
    }

    [Fact]
    public async Task Handle_UnsuccessfulMatch_ContinuesPipeline()
    {
        // Arrange
        
        var mockNextDelegate = Substitute.For<RequestHandlerDelegate<PipelineResult>>();
        mockNextDelegate.Invoke().Returns(new PipelineResult(true));
        
        var stubService = Substitute.For<PipelineTestHelpers.IStubService>();
        stubService.ProcessFilter(Arg.Any<PipelineTestHelpers.MockContext>()).Returns(new PipelineResult(false));

        var sut = new PipelineTestHelpers.MockPipeline(stubService);
        var request = new PipelineTestHelpers.MockRequest(new PipelineTestHelpers.MockContext());
        
        // Act
        var result = await sut.Handle(request, mockNextDelegate, CancellationToken.None);
        
        // Assert
        result.ExitPipeline.Should().BeTrue();
        await mockNextDelegate.Received(1).Invoke();
    }
}