namespace Btms.Business.Pipelines.PreProcessing;

public record PreProcessingContext<T>(T Message, string MessageId);