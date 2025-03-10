using Btms.Model.Validation;

namespace Btms.Validation;

public class BtmsValidationResult
{
    private List<BtmsValidationFailure> _errors;

    /// <summary>
    /// Whether validation succeeded
    /// </summary>
    public virtual bool IsValid => Errors.Count == 0;

    /// <summary>
    /// A collection of errors
    /// </summary>
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

    /// <summary>
    /// Creates a new ValidationResult from a collection of failures
    /// </summary>
    /// <param name="failures">Collection of <see cref="ValidationFailure"/> instances which is later available through the <see cref="Errors"/> property.</param>
    /// <remarks>
    /// Any nulls will be excluded.
    /// The list is copied.
    /// </remarks>
    public BtmsValidationResult(IEnumerable<BtmsValidationFailure> failures)
    {
        _errors = failures.Where(failure => failure != null).ToList();
    }

    /// <summary>
    /// Generates a string representation of the error messages separated by new lines.
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
        return ToString(Environment.NewLine);
    }

    /// <summary>
    /// Generates a string representation of the error messages separated by the specified character.
    /// </summary>
    /// <param name="separator">The character to separate the error messages.</param>
    /// <returns></returns>
    public string ToString(string separator)
    {
        return string.Join(separator, _errors.Select(failure => failure.ErrorMessage));
    }

    /// <summary>
    /// Converts the ValidationResult's errors collection into a simple dictionary representation.
    /// </summary>
    /// <returns>A dictionary keyed by property name
    /// where each value is an array of error messages associated with that property.
    /// </returns>
    public IDictionary<string, string[]> ToDictionary()
    {
        return Errors
            .GroupBy(x => x.PropertyName)
            .ToDictionary(
                g => g.Key,
                g => g.Select(x => x.ErrorMessage).ToArray()
            );
    }
}