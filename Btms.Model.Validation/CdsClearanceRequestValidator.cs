using Btms.Model.Cds;
using Btms.Validation;
using FluentValidation;

namespace Btms.Model.Validation
{
    public class CdsClearanceRequestValidator : AbstractValidator<BtmsValidationPair<CdsClearanceRequest, Movement>>
    {
        public CdsClearanceRequestValidator()
        {
            When(p => p.ExistingRecord is not null, () =>
            {
                RuleFor(p => p.NewRecord).Must(NotBeADuplicateEntryVersionNumber)
                    .WithState(p => "ALVSVAL303")
                    .WithMessage(p => $"The import declaration was received as a new declaration. There is already a current import declaration in BTMS with EntryReference {p.ExistingRecord.Id} and EntryVersionNumber.Your request with Correlation ID {p.NewRecord.ServiceHeader?.CorrelationId} has been terminated.");

                RuleFor(p => p.ExistingRecord).Must(NotBeCancelled)
                    .WithState(p => "ALVSVAL324")
                    .WithMessage(p =>
                        $"The Import Declaration with Entry Reference {p.NewRecord.Header?.EntryVersionNumber} and EntryVersionNumber {p.NewRecord.Header?.EntryVersionNumber} was received but the Import Declaration was cancelled. Your request with correlation ID {p.NewRecord.ServiceHeader?.CorrelationId} has been terminated.");
            });
        }

        private bool NotBeADuplicateEntryVersionNumber(BtmsValidationPair<CdsClearanceRequest, Movement> pair, CdsClearanceRequest clearanceRequest)
        {
            return pair.ExistingRecord?.EntryVersionNumber == clearanceRequest.Header?.EntryVersionNumber;
        }

        private bool NotBeCancelled(Movement movement)
        {
            return (movement.Finalisation?.FinalState.IsCancelled()).GetValueOrDefault();
        }
    }
}
