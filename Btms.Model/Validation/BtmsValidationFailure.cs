using Btms.Common.Extensions;

namespace Btms.Model.Validation;

public class BtmsValidationFailure
{
    public BtmsValidationFailure(string propertyName, string errorMessage, string errorCode,
        ValidationSeverity severity = ValidationSeverity.Error) : this(propertyName, errorMessage, errorCode, null,
        severity)
    {
    }

    public BtmsValidationFailure(string propertyName, string errorMessage, string errorCode, object? attemptedValue,
        ValidationSeverity severity = ValidationSeverity.Error)
    {
        PropertyName = propertyName;
        ErrorMessage = errorMessage;
        ErrorCode = errorCode;
        AttemptedValue = attemptedValue?.ToJson();
        Severity = severity;
    }

    public string PropertyName { get; private set; }

    public string ErrorMessage { get; private set; }

    public object? AttemptedValue { get; private set; }

    public ValidationSeverity Severity { get; private set; }

    public string ErrorCode { get; private set; }

    public override string ToString()
    {
        return ErrorMessage;
    }
}