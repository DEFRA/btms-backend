using FluentValidation;

namespace Btms.Types.Alvs.Validation;

public class ServiceHeaderValidator : AbstractValidator<ServiceHeader>
{
    public ServiceHeaderValidator()
    {
        RuleFor(p => p.SourceSystem).NotNull().NotEmpty();
        RuleFor(p => p.DestinationSystem).NotNull().NotEmpty();
        RuleFor(p => p.CorrelationId).NotNull().NotEmpty();
        RuleFor(p => p.ServiceCallTimestamp).NotNull();
    }
}