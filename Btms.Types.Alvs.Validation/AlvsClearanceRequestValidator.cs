using FluentValidation;

namespace Btms.Types.Alvs.Validation
{
    public class AlvsClearanceRequestValidator : AbstractValidator<AlvsClearanceRequest>
    {
        public AlvsClearanceRequestValidator()
        {
            RuleFor(p => p.ServiceHeader).SetValidator(new ServiceHeaderValidator());
            RuleFor(p => p.Header).SetValidator(new HeaderValidator());
            RuleForEach(p => p.Items).SetValidator(new ItemsValidator());
        }
    }
}
