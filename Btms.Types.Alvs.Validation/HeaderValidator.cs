using FluentValidation;

namespace Btms.Types.Alvs.Validation;

public class HeaderValidator : AbstractValidator<Header?>
{
    public HeaderValidator(object correlationId)
    {
        RuleFor(p => p!.EntryReference)
            .NotEmpty()
            .MaximumLength(22)
            .Matches("[1-9]{2}[A-Za-z]{2}[A-Za-z0-9]{14}")
            .WithState(p => "ALVSVAL303");

        RuleFor(p => p!.EntryVersionNumber).NotNull()
            .WithState(p => "ALVSVAL153")
            .WithMessage(
                $"EntryVersionNumber has not been provided for the import document. Provide an EntryVersionNumber. Your request with correlation ID {correlationId} has been terminated.");

        RuleFor(p => p!.EntryVersionNumber).InclusiveBetween(1, 99);

        RuleFor(p => p!.PreviousVersionNumber).NotNull().WithState(p => "ALVSVAL152")
            .WithMessage($"PreviousVersionNumber has not been provided for the import document. Provide a PreviousVersionNumber. Your request with correlation ID {correlationId} has been terminated.")
            .When(p => p!.EntryVersionNumber > 1);

        RuleFor(p => p).Must(p => p!.EntryVersionNumber > p.PreviousVersionNumber)
            .WithState(p => "ALVSVAL326")
            .WithMessage(p => $"The previous version number {p!.PreviousVersionNumber} on the entry document must be less than the entry version number. Your service request with Correlation ID {correlationId} has been terminated.")
            .When(p => p!.PreviousVersionNumber.HasValue);

        RuleFor(p => p!.DeclarationUcr).NotEmpty()
            .WithState(p => "ALVSVAL313")
            .WithMessage($"DeclarationUcr has not been provided for the import document. Provide an DeclarationUcr. Your request with correlation ID {correlationId} has been terminated.");
        
        RuleFor(p => p!.DeclarationUcr).MaximumLength(35);

        RuleFor(p => p!.DeclarantId).NotEmpty().MaximumLength(18);
        RuleFor(p => p!.DeclarantName).NotEmpty().MaximumLength(35);
        RuleFor(p => p!.DispatchCountryCode).NotEmpty().Length(2);

        RuleFor(p => p!.DeclarationType).Must(p => p == "S" || p == "F");
    }
}