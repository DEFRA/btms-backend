using FluentValidation;

namespace Btms.Types.Alvs.Validation;

public class HeaderValidator : AbstractValidator<Header>
{
    public HeaderValidator()
    {
        RuleFor(p => p.EntryReference).NotNull().NotEmpty();
        RuleFor(p => p.EntryVersionNumber).NotNull();
        RuleFor(p => p.DeclarationUcr).NotNull().NotEmpty();
        RuleFor(p => p.DeclarantId).NotNull().NotEmpty();
        RuleFor(p => p.DeclarantName).NotNull().NotEmpty();
        RuleFor(p => p.DispatchCountryCode).NotNull().NotEmpty();
    }
}