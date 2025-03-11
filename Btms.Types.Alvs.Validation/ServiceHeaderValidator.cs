using FluentValidation;

namespace Btms.Types.Alvs.Validation;

public class ServiceHeaderValidator : AbstractValidator<ServiceHeader>
{
    public ServiceHeaderValidator()
    {
        RuleFor(p => p.SourceSystem).Must(p => p == "CDS")
            .WithMessage(c => $"Source system {c.SourceSystem} is invalid. Your request with correlation ID {c.CorrelationId} has been terminated.")
            .WithErrorCode("ALVSVAL101");
        RuleFor(p => p.DestinationSystem).Must(p => p == "ALVS")
            .WithMessage(c => $"Destination system {c.DestinationSystem} is invalid. Your request with correlation ID {c.CorrelationId} has been terminated.")
            .WithErrorCode("ALVSVAL102");

        RuleFor(p => p.CorrelationId).NotEmpty().MaximumLength(20);
        RuleFor(p => p.ServiceCallTimestamp).NotNull();
    }
}