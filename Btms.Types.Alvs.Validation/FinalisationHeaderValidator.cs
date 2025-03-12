using FluentValidation;

namespace Btms.Types.Alvs.Validation;

public class FinalisationHeaderValidator : AbstractValidator<FinalisationHeader>
{
    public FinalisationHeaderValidator()
    {
        RuleFor(p => p.EntryReference)
            .NotEmpty()
            .MaximumLength(22)
            .Matches("[1-9]{2}[A-Za-z]{2}[A-Za-z0-9]{14}")
            .WithErrorCode("ALVSVAL401");
        RuleFor(p => p.EntryVersionNumber).NotNull();

        RuleFor(p => p.FinalState).NotNull();

        RuleFor(p => p.ManualAction).Must(p => p == "N" || p == "Y");
    }
}