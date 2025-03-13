using Btms.Model.Validation;

namespace Btms.Validation;

public interface IBtmsValidator
{
    BtmsValidationResult Validate<T>(T entity);
}