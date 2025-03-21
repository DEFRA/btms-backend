using FluentValidation;

namespace Btms.Types.Alvs.Validation;

public class CheckValidator : AbstractValidator<Check>
{
    public CheckValidator(string correlationId)
    {
        RuleFor(p => p.CheckCode).NotEmpty()
            .WithMessage(c => $"The CheckCode field on item number {c.CheckCode} must have a value. Your service request with Correlation ID {correlationId} has been terminated.")
            .WithState(p => "ALVSVAL311");

        RuleFor(p => p.CheckCode).MaximumLength(4);
        RuleFor(p => p.DepartmentCode).NotEmpty().MaximumLength(8);
    }
}