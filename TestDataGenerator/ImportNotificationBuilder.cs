using Cdms.Common.Extensions;
using Cdms.Types.Ipaffs;
using TestDataGenerator.Helpers;

namespace TestDataGenerator;

public class ImportNotificationBuilder : ImportNotificationBuilder<ImportNotification>
{
    private ImportNotificationBuilder()
    {
    }

    private ImportNotificationBuilder(string file) : base(file)
    {
    }

    public static ImportNotificationBuilder<ImportNotification> FromFile(string file)
    {
        return new ImportNotificationBuilder(file)
            .WithClean();
    }

    public static ImportNotificationBuilder<ImportNotification> Default()
    {
        return new ImportNotificationBuilder()
            .WithClean();
    }
}

public class ImportNotificationBuilder<T> : BuilderBase<T, ImportNotificationBuilder<T>>
    where T : ImportNotification, new()
{
    protected ImportNotificationBuilder()
    {
    }

    protected ImportNotificationBuilder(string file) : base(file)
    {
    }

    /// <summary>
    ///     Allows any customisations needed, such as removing problems with serialisation, e.g Do(n =>
    ///     Array.ForEach(n.PartOne!.Commodities!.ComplementParameterSets!, x => x.KeyDataPairs = null));
    /// </summary>
    protected ImportNotificationBuilder<T> WithClean()
    {
        // TODO : 

        return this;
    }

    public ImportNotificationBuilder<T> WithRandomCommodities(int min, int max)
    {
        var commodityCount = CreateRandomInt(min, max);

        return Do(n =>
        {
            var commodities = Enumerable.Range(0, commodityCount)
                .Select(x => n.PartOne!.Commodities!.CommodityComplements![0]
                ).ToArray();

            n.PartOne!.Commodities!.CommodityComplements = commodities;
        });
    }

    public ImportNotificationBuilder<T> WithReferenceNumber(ImportNotificationTypeEnum chedType, int scenario,
        DateTime created, int item)
    {
        var prefix = chedType switch
        {
            ImportNotificationTypeEnum.Cveda => "CHEDA",
            ImportNotificationTypeEnum.Cvedp => "CHEDP",
            ImportNotificationTypeEnum.Chedpp => "CHEDPP",
            ImportNotificationTypeEnum.Ced => "CHEDD",
            _ => throw new ArgumentOutOfRangeException(nameof(chedType), chedType, null)
        };

        if (item > 999999) throw new ArgumentException("Currently only deals with max 100,000 items");

        var formatHundredThousands = "000000";

        return Do(x =>
            x.ReferenceNumber =
                $"{prefix}.GB.{created.Year}.{scenario.ToString("00")}{created.DateRef()}{(item + 1).ToString(formatHundredThousands)}");
    }

    public ImportNotificationBuilder<T> WithEntryDate(DateTime entryDate)
    {
        return Do(x => x.LastUpdated = entryDate);
    }

    public override ImportNotificationBuilder<T> Validate()
    {
        return Do(n => { n.ReferenceNumber.AssertHasValue(); });
    }
}