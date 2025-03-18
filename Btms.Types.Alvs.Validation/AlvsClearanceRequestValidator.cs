using FluentValidation;

namespace Btms.Types.Alvs.Validation
{
    public class AlvsClearanceRequestValidator : AbstractValidator<AlvsClearanceRequest>
    {
        public AlvsClearanceRequestValidator()
        {
            RuleFor(p => p.ServiceHeader).SetValidator(new ServiceHeaderValidator()!);
            RuleFor(p => p.Header).SetValidator(p => new HeaderValidator(p?.ServiceHeader?.CorrelationId!));
            RuleForEach(p => p.Items).SetValidator(p => new ItemsValidator(p?.ServiceHeader?.CorrelationId!));
        }
    }
}