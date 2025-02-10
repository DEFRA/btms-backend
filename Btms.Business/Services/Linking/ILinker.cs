namespace Btms.Business.Services.Linking;

// ReSharper disable once UnusedTypeParameter
public interface ILinker<TModel, in TKModel> where TModel : class where TKModel : class
{
    Task Link(TKModel model, CancellationToken cancellationToken);
}