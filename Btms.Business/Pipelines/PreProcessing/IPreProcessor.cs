using Btms.Model.Data;

namespace Btms.Business.Pipelines.PreProcessing;

public interface IPreProcessor<TInput, TOutput> where TOutput : IAuditable
{
    Task<PreProcessingResult<TOutput>> Process(PreProcessingContext<TInput> preProcessingContext, CancellationToken cancellationToken = default);
}