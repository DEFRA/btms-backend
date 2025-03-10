using FluentValidation;

namespace Btms.Types.Alvs.Validation;

public class HeaderValidator : AbstractValidator<Header>
{
    public HeaderValidator()
    {
        RuleFor(p => p.EntryReference).NotEmpty();
        RuleFor(p => p.EntryVersionNumber).NotNull();
        RuleFor(p => p.DeclarationUcr).NotEmpty();
        RuleFor(p => p.DeclarantId).NotEmpty();
        RuleFor(p => p.DeclarantName).NotEmpty();
        RuleFor(p => p.DispatchCountryCode).NotEmpty();
    }
}