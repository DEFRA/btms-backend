namespace Btms.Model.Validation;

public class BtmsValidationResult
{
    private List<BtmsValidationFailure> _errors;

    public virtual bool IsValid => Errors.Count == 0;

    public List<BtmsValidationFailure> Errors
    {
        get => _errors;
        set
        {
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            // Ensure any nulls are removed and the list is copied
            // to be consistent with the constructor below.
            _errors = value.Where(failure => failure != null).ToList();
        }
    }

    public BtmsValidationResult(IEnumerable<BtmsValidationFailure> failures)
    {
        _errors = failures.Where(failure => failure != null).ToList();
    }
}