namespace Btms.Validation
{
    public record BtmsValidationPair<T>(T NewRecord, T? ExistingRecord);
}
