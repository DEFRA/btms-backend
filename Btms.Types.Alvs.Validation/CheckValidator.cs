using FluentValidation;

namespace Btms.Types.Alvs.Validation;

public class CheckValidator : AbstractValidator<Check>
{
    public CheckValidator()
    {
        RuleFor(p => p.CheckCode).NotEmpty();
        RuleFor(p => p.DepartmentCode).NotEmpty();
    }
}