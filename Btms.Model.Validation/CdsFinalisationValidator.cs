using Btms.Model.Cds;
using Btms.Validation;
using FluentValidation;

namespace Btms.Model.Validation;

public class CdsFinalisationValidator : AbstractValidator<BtmsValidationPair<CdsFinalisation, Movement>>
{
    public CdsFinalisationValidator()
    {
        When(p => p.ExistingRecord is not null, () =>
        {
            RuleFor(p => p.ExistingRecord).Must(NotBeDuplicateFinalise!)
                .WithState(p => "ALVSVAL501")
                .WithMessage(p =>
                    $"The Import Declaration with Entry Reference {p.NewRecord.Header?.EntryVersionNumber} and EntryVersionNumber {p.NewRecord.Header?.EntryVersionNumber} was received but the Import Declaration was cancelled. Your request with correlation ID {p.NewRecord.ServiceHeader?.CorrelationId} has been terminated.");

            RuleFor(p => p.ExistingRecord).Must(NotBeCancelled!)
                .WithState(p => "ALVSVAL403")
                .WithMessage(p =>
                    $"The Import Declaration with Entry Reference {p.NewRecord.Header?.EntryVersionNumber} and EntryVersionNumber {p.NewRecord.Header?.EntryVersionNumber} was received but the Import Declaration was cancelled. Your request with correlation ID {p.NewRecord.ServiceHeader?.CorrelationId} has been terminated.");

            RuleFor(p => p.NewRecord).Must(NotBeADuplicateEntryVersionNumber)
                .WithState(p => "ALVSVAL401")
                .WithMessage(p =>
                    $"The finalised state was received for EntryReference {p.NewRecord.Header?.EntryVersionNumber} EntryVersionNumber {p.NewRecord.Header?.EntryVersionNumber}. This has already been replaced by a later version of the import declaration. Your request with correlation ID {p.NewRecord.ServiceHeader?.CorrelationId} has been terminated.");

            RuleFor(p => p.NewRecord).Must(BeValidCancelRequest)
                .WithState(p => "ALVSVAL506")
                .WithMessage(p =>
                    $" The import declaration was received as a cancellation. The EntryReference {p.NewRecord.Header?.EntryVersionNumber} EntryVersionNumber {p.NewRecord.Header?.EntryVersionNumber} have already been replaced by a later version. Your request with correlation ID {p.NewRecord.ServiceHeader?.CorrelationId} has been terminated.");
        });
    }

    private static bool NotBeDuplicateFinalise(BtmsValidationPair<CdsFinalisation, Movement> pair, Movement movement)
    {
        var movementCancelled = (movement.Finalisation?.FinalState.IsCancelled()).GetValueOrDefault();
        return !(pair.NewRecord.Header.FinalState.IsCancelled() && movementCancelled);
    }

    private static bool NotBeADuplicateEntryVersionNumber(BtmsValidationPair<CdsFinalisation, Movement> pair, CdsFinalisation finalisation)
    {
        return !finalisation.Header.FinalState.IsCancelled() && pair.ExistingRecord?.EntryVersionNumber == finalisation.Header?.EntryVersionNumber;
    }

    private static bool BeValidCancelRequest(BtmsValidationPair<CdsFinalisation, Movement> pair, CdsFinalisation finalisation)
    {
        return !(finalisation.Header.FinalState.IsCancelled() && pair.ExistingRecord?.EntryVersionNumber == finalisation.Header?.EntryVersionNumber);
    }

    private static bool NotBeCancelled(Movement movement)
    {
        return !(movement.Finalisation?.FinalState.IsCancelled()).GetValueOrDefault();
    }
}