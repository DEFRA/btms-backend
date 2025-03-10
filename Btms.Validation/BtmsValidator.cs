using Btms.Model.Validation;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace Btms.Validation;

public class BtmsValidator(IServiceProvider serviceProvider) : IBtmsValidator
{
    public BtmsValidationResult Validate<T>(T entity)
    {
        var validators = serviceProvider.GetServices<IValidator<T>>().ToList();

        if (!validators.Any())
        {
            throw new NotSupportedException("No validator found");
        }

        var validationFailures = validators.Select(validator => validator.Validate(entity));

        var errors = validationFailures
            .Where(validationResult => !validationResult.IsValid)
            .SelectMany(validationResult => validationResult.Errors)
            .Select(validationFailure => new BtmsValidationFailure(
                validationFailure.PropertyName,
                validationFailure.ErrorMessage,
                validationFailure.ErrorCode,
                validationFailure.AttemptedValue,
                Enum.Parse<ValidationSeverity>(validationFailure.Severity.ToString())))
            .ToList();

        return new BtmsValidationResult(errors);
    }
}