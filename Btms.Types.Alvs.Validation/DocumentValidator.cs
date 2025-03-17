using FluentValidation;

namespace Btms.Types.Alvs.Validation;

public class DocumentValidator : AbstractValidator<Document>
{
    public DocumentValidator(int? itemNumber, string correlationId)
    {
        RuleFor(p => p.DocumentCode).Must(BeAValidDocumentCode!)
            .WithMessage(c => $"DocumentCode {c.DocumentCode} on item number {itemNumber} is invalid. Your request with correlation ID {correlationId} has been terminated.")
            .WithState(p => "ALVSVAL308");
        RuleFor(p => p.DocumentStatus).NotEmpty();
        RuleFor(p => p.DocumentControl).NotEmpty();
    }

    private static List<string> documentCodes =
        ["C633", "C640", "C641", "C673", "N002", "N851", "N852", "C678", "N853", "9HCG", "9115"];
    private static bool BeAValidDocumentCode(string documentCode)
    {
        return !string.IsNullOrEmpty(documentCode) && documentCodes.Contains(documentCode);
    }
}