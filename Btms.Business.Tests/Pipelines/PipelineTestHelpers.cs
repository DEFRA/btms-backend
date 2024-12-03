using Btms.Business.Pipelines;

namespace Btms.Business.Tests.Pipelines;

public class PipelineTestHelpers
{
    public interface IStubService
    {
        PipelineResult ProcessFilter(MockContext context);
    }
    
    public class MockPipeline : PipelineBase<MockContext, MockRequest>
    {
        private readonly IStubService _stubService;
        
        public MockPipeline(IStubService stubService)
        {
            _stubService = stubService;
        }
        
        public override async Task<PipelineResult> ProcessFilter(MockContext context)
        {
            return await Task.FromResult(_stubService.ProcessFilter(context));
        }
    }

    public class MockTerminatePipeline : TerminatePipelineBase<MockRequest>;

    public record MockRequest(MockContext Context) : PipelineRequest<MockContext>(Context);

    public record MockContext;
}