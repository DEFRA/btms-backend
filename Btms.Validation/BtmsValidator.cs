using Btms.Common.FeatureFlags;
using Btms.Model.Validation;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.FeatureManagement;

namespace Btms.Validation;

public class BtmsValidator(IServiceProvider serviceProvider, IFeatureManager featureManager) : IBtmsValidator
{
    public BtmsValidationResult Validate<T>(T entity, string? friendlyName = null)
    {
        var name = string.IsNullOrEmpty(friendlyName) ? typeof(T).Name : friendlyName;
        if (!featureManager.IsEnabledAsync($"{Features.Validation}_{name}").GetAwaiter().GetResult())
        {
            return new BtmsValidationResult([]);
        }

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
                new BtmsValidationFailureContext() { CdsErrorCode = validationFailure.CustomState?.ToString() },
                Enum.Parse<ValidationSeverity>(validationFailure.Severity.ToString())))
            .ToList();

        return new BtmsValidationResult(errors);
    }
}