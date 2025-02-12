using System.Diagnostics.CodeAnalysis;

namespace Btms.Business.Services.Linking;

// ReSharper disable once UnusedTypeParameter
[SuppressMessage("SonarLint", "S2326",
    Justification =
        "TModel is not used in the interface but it's there to allow different from and to linkers")]
public interface ILinker<TModel, in TKModel> where TModel : class where TKModel : class
{
    Task Link(TKModel model, CancellationToken cancellationToken);
}