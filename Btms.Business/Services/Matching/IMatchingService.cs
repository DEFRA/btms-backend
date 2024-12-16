namespace Btms.Business.Services.Matching;

public interface IMatchingService
{
    public Task<MatchingResult> Process(MatchingContext matchingContext, CancellationToken cancellationToken);
}