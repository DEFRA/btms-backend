using FluentValidation;

namespace Btms.Types.Alvs.Validation;

public class FinalisationValidator : AbstractValidator<Finalisation>
{
    public FinalisationValidator()
    {
        RuleFor(p => p.ServiceHeader).SetValidator(new ServiceHeaderValidator());
        RuleFor(p => p.Header).SetValidator(new FinalisationHeaderValidator());
    }
}