namespace Btms.Business.Services.Linking;

public interface ILinker<TFrom, TTo> where TFrom : class where TTo : class
{
    Task<LinkerResult<TFrom, TTo>> Link(TTo model, CancellationToken cancellationToken);
}