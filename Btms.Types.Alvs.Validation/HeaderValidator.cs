using FluentValidation;

namespace Btms.Types.Alvs.Validation;

public class HeaderValidator : AbstractValidator<Header>
{
    public HeaderValidator(object correlationId)
    {
        RuleFor(p => p.EntryReference).NotEmpty();
        RuleFor(p => p.EntryVersionNumber).NotNull()
            .WithErrorCode("ALVSVAL153")
            .WithMessage($"EntryVersionNumber has not been provided for the import document. Provide an EntryVersionNumber. Your request with correlation ID {correlationId} has been terminated.");

 
        RuleFor(p => p.PreviousVersionNumber).NotNull().WithErrorCode("ALVSVAL152")
            .WithMessage($"PreviousVersionNumber has not been provided for the import document. Provide a PreviousVersionNumber. Your request with correlation ID {correlationId} has been terminated.")
            .When(p => p.EntryVersionNumber > 1);

        RuleFor(p => p).Must(p => p.EntryVersionNumber > p.PreviousVersionNumber)
            .WithErrorCode("ALVSVAL326")
            .WithMessage(p => $"The previous version number {p.PreviousVersionNumber} on the entry document must be less than the entry version number. Your service request with Correlation ID {correlationId} has been terminated.")
            .When(p => p.PreviousVersionNumber.HasValue);

        RuleFor(p => p.DeclarationUcr).NotEmpty()
            .WithErrorCode("ALVSVAL313")
            .WithMessage($"DeclarationUcr has not been provided for the import document. Provide an DeclarationUcr. Your request with correlation ID {correlationId} has been terminated.");

        RuleFor(p => p.DeclarantId).NotEmpty();
        RuleFor(p => p.DeclarantName).NotEmpty();
        RuleFor(p => p.DispatchCountryCode).NotEmpty().Length(2);

        RuleFor(p => p.DeclarationType).Must(p => p == "S" || p == "F");
    }
}