namespace Btms.Validation
{
    public record BtmsValidationPair<T>(T NewRecord, T? ExistingRecord);

    public record BtmsValidationPair<TNew, TExisting>(TNew NewRecord, TExisting? ExistingRecord);
}