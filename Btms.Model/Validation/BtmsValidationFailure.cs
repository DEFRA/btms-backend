using Btms.Validation;

namespace Btms.Model.Validation;

/// <summary>
/// Defines a validation failure
/// </summary>
[Serializable]
public class BtmsValidationFailure
{
    /// <summary>
    /// Creates a new validation failure.
    /// </summary>
    public BtmsValidationFailure(string propertyName, string errorMessage, string errorCode, ValidationSeverity severity = ValidationSeverity.Error) : this(propertyName, errorMessage, errorCode, null, severity)
    {

    }

    /// <summary>
    /// Creates a new ValidationFailure.
    /// </summary>
    public BtmsValidationFailure(string propertyName, string errorMessage, string errorCode, object? attemptedValue, ValidationSeverity severity = ValidationSeverity.Error)
    {
        PropertyName = propertyName;
        ErrorMessage = errorMessage;
        ErrorCode = errorCode;
        AttemptedValue = attemptedValue;
        Severity = severity;
    }

    /// <summary>
    /// The name of the property.
    /// </summary>
    public string PropertyName { get; private set; }

    /// <summary>
    /// The error message
    /// </summary>
    public string ErrorMessage { get; private set; }

    /// <summary>
    /// The property value that caused the failure.
    /// </summary>
    public object? AttemptedValue { get; private set; }

    /// <summary>
    /// Custom severity level associated with the failure.
    /// </summary>
    public ValidationSeverity Severity { get; private set; }

    /// <summary>
    /// Gets or sets the error code.
    /// </summary>
    public string ErrorCode { get; private set; }

    /// <summary>
    /// Creates a textual representation of the failure.
    /// </summary>
    public override string ToString()
    {
        return ErrorMessage;
    }
}