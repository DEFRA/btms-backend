using FluentValidation;

namespace Btms.Types.Alvs.Validation;

public class DocumentValidator : AbstractValidator<Document>
{
    public DocumentValidator()
    {
        RuleFor(p => p.DocumentCode).NotNull().NotEmpty();
        RuleFor(p => p.DocumentStatus).NotNull().NotEmpty();
        RuleFor(p => p.DocumentControl).NotNull().NotEmpty();
    }
}