using Btms.Common.Extensions;

namespace Btms.Model.Validation;

public class BtmsValidationFailureContext
{
    public string? CdsErrorCode { get; set; }
}

public class BtmsValidationFailure
{
    public BtmsValidationFailure(string propertyName, string errorMessage, string errorCode,
        ValidationSeverity severity = ValidationSeverity.Error) : this(propertyName, errorMessage, errorCode, null,
        null, severity)
    {
    }

    public BtmsValidationFailure(string propertyName, string errorMessage, string errorCode, object? attemptedValue,
        BtmsValidationFailureContext? context, ValidationSeverity severity = ValidationSeverity.Error)
    {
        PropertyName = propertyName;
        ErrorMessage = errorMessage;
        ErrorCode = errorCode;
        AttemptedValue = attemptedValue?.ToJson();
        Context = context;
        Severity = severity;
    }

    public string PropertyName { get; private set; }

    public string ErrorMessage { get; private set; }

    public object? AttemptedValue { get; private set; }

    public BtmsValidationFailureContext? Context { get; private set; }

    public ValidationSeverity Severity { get; private set; }

    public string ErrorCode { get; private set; }

    public override string ToString()
    {
        return ErrorMessage;
    }
}