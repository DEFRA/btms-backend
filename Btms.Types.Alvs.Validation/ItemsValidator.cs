using FluentValidation;

namespace Btms.Types.Alvs.Validation;

public class ItemsValidator : AbstractValidator<Items>
{
    public ItemsValidator(string correlationId)
    {
        RuleFor(p => p.ItemNumber).NotNull().InclusiveBetween(1, 999);
        RuleFor(p => p.CustomsProcedureCode).NotEmpty().MaximumLength(7);
        RuleFor(p => p.TaricCommodityCode).NotEmpty().Matches("\\d").Length(10);
        RuleFor(p => p.GoodsDescription).NotEmpty().MaximumLength(280);

        RuleFor(p => p.ConsigneeId).NotEmpty().MaximumLength(18);
        RuleFor(p => p.ConsigneeName).NotEmpty().MaximumLength(35);
        RuleFor(p => p.ItemNetMass).NotNull();
        RuleFor(p => p.ItemOriginCountryCode).NotNull().Length(2);

        RuleForEach(p => p.Documents).SetValidator(item => new DocumentValidator(item.ItemNumber, correlationId));
        RuleForEach(p => p.Checks).SetValidator(new CheckValidator(correlationId));

        RuleForEach(p => p.Documents).Must(MustHaveCorrectDocumentCodesForChecks)
            .WithMessage((item, document) => $"Document code{document.DocumentCode} is not appropriate for the check code requested on ItemNumber {item.ItemNumber}. Your request with correlation ID {correlationId} has been terminated.")
            .WithState(p => "ALVSVAL320")
            .When(p => p.Checks is not null);

        RuleForEach(p => p.Checks).Must(MustHaveDocumentForCheck)
            .WithMessage((item, check) => $"Check code {check.CheckCode} on ItemNumber {item.ItemNumber} must have a document code. Your request with Correlation ID {correlationId} has been terminated.")
            .WithState(p => "ALVSVAL321");

        RuleFor(p => p.Checks).Must(MustOnlyHaveOneCheckPerAuthority!)
            .WithMessage(p => $"Item {p.ItemNumber} has more than one Item Check defined for the same authority. You can only provide one. Your service request with Correlation ID {correlationId} has been terminated.")
            .WithState(p => "ALVSVAL317")
            .When(p => p.Checks is not null);

        RuleFor(p => p.Checks).Must(MustHavePoAoCheck!)
            .WithMessage(p => $"An IUU document has been specified for ItemNumber {p.ItemNumber}. Request a manual clearance if the item does not require a CHED P. Your request with correlation ID {correlationId} has been terminated.")
            .WithState(p => "ALVSVAL328")
            .When(x => x.Checks is not null && x.Checks.Any(x => x.CheckCode == "H224"));

        RuleFor(p => p.Documents).NotEmpty()
            .WithMessage(c => $"Item {c.ItemNumber} has no document code. BTMS requires at least one item document. Your request with correlation ID {correlationId} has been terminated..")
            .WithState(p => "ALVSVAL308");
    }

    private static bool MustHaveCorrectDocumentCodesForChecks(Items item, Document document)
    {
        var checkCodes = AuthorityCodeMappings.Where(x => x.DocumentCode == document.DocumentCode).Select(x => x.CheckCode);
        return item.Checks != null && item.Checks.Any(x => checkCodes.Contains(x.CheckCode));
    }

    private static bool MustHaveDocumentForCheck(Items item, Check check)
    {
        var documentCodes = AuthorityCodeMappings.Where(x => x.CheckCode == check.CheckCode).Select(x => x.DocumentCode);
        return item.Documents != null && item.Documents.Any(x => documentCodes.Contains(x.DocumentCode));
    }

    private static bool MustOnlyHaveOneCheckPerAuthority(Items item, Check[] checks)
    {
        var authorityCheckCodes = AuthorityCodeMappings.GroupBy(x => x.Name)
            .ToDictionary(g => g.Key, g => g.Select(x => x.CheckCode).Distinct().ToList());

        return authorityCheckCodes.All(authorityCheckCode =>
            checks.Count(x => x.CheckCode != null && authorityCheckCode.Value.Contains(x.CheckCode)) <= authorityCheckCode.Value.Count);
    }

    private static bool MustHavePoAoCheck(Items item, Check[] checks)
    {
        return checks.Any(x => x.CheckCode == "H222");
    }

    private sealed record AuthorityCodeMap(string Name, string DocumentCode, string CheckCode);

    private static List<AuthorityCodeMap> AuthorityCodeMappings = new()
    {
        new AuthorityCodeMap("hmi", "N002", "H218"),
        new AuthorityCodeMap("hmi", "N002", "H220"),
        new AuthorityCodeMap("hmi", "C085", "H218"),
        new AuthorityCodeMap("hmi", "C085", "H220"),

        new AuthorityCodeMap("phsi", "N851", "H219"),
        new AuthorityCodeMap("phsi", "9115", "H219"),
        new AuthorityCodeMap("phsi", "C085", "H219"),

        new AuthorityCodeMap("pha", "C673", "H224"),
        new AuthorityCodeMap("pha", "C641", "H224"),

        new AuthorityCodeMap("pha", "N853", "H222"),

        new AuthorityCodeMap("pha", "C678", "H223"),

        new AuthorityCodeMap("apha", "C640", "H221")

    };
}