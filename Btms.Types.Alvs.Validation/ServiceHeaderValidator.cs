using FluentValidation;

namespace Btms.Types.Alvs.Validation;

public class ServiceHeaderValidator : AbstractValidator<ServiceHeader>
{
    public ServiceHeaderValidator()
    {
        RuleFor(p => p.SourceSystem).NotEmpty();
        RuleFor(p => p.DestinationSystem).NotEmpty();
        RuleFor(p => p.CorrelationId).NotEmpty();
        RuleFor(p => p.ServiceCallTimestamp).NotNull();
    }
}