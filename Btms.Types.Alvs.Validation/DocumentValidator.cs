using FluentValidation;

namespace Btms.Types.Alvs.Validation;

public class DocumentValidator : AbstractValidator<Document>
{
    public DocumentValidator()
    {
        RuleFor(p => p.DocumentCode).NotEmpty();
        RuleFor(p => p.DocumentStatus).NotEmpty();
        RuleFor(p => p.DocumentControl).NotEmpty();
    }
}