using System.ComponentModel.DataAnnotations;
using Btms.Common.Extensions;

namespace Btms.Common.Options;
/// <summary>
/// Borrowed from https://stackoverflow.com/questions/26354853/conditionally-required-property-using-data-annotations
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter, AllowMultiple = false)]
public class RequiredIfAttribute : ValidationAttribute
{
    public string PropertyName { get; set; }
    public object? Value { get; set; }

    public RequiredIfAttribute(string propertyName, object? value = null)
    {
        PropertyName = propertyName;
        Value = value;
    }

    protected override ValidationResult IsValid(object? value, ValidationContext validationContext)
    {
        if (PropertyName == null || PropertyName.ToString() == "")
        {
            throw new ArgumentException("RequiredIf: you have to indicate the name of the property to use in the validation");
        }

        var propertyValue = GetPropertyValue(validationContext);

        if (HasPropertyValue(propertyValue) && (value == null || value.ToString() == ""))
        {
            return new ValidationResult($"{validationContext.DisplayName} is required if {PropertyName} = {Value}");
        }
        else
        {
            return ValidationResult.Success!;
        }
    }

    private object? GetPropertyValue(ValidationContext validationContext)
    {
        var instance = validationContext.ObjectInstance;
        var type = instance.GetType();
        return type.GetProperty(PropertyName)!.GetValue(instance);
    }

    private bool HasPropertyValue(object? propertyValue)
    {
        if (Value.HasValue())
        {
            return propertyValue.HasValue() && propertyValue.ToString() == Value.ToString();
        }
        else
        {
            return propertyValue.HasValue() && propertyValue.ToString() != "";
        }
    }
}