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
    protected ImportNotificationBuilder() : base()
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
        return this;
    }

    public ImportNotificationBuilder<T> WithRandomCommodities(int min, int max)
    {
        var commodityCount = CreateRandomInt(min, max);

        return Do(n =>
        {
            var commodities = Enumerable.Range(0, commodityCount)
                .Select(_ => n.PartOne!.Commodities!.CommodityComplements![0]
                ).ToArray();

            n.PartOne!.Commodities!.CommodityComplements = commodities;
        });
    }
    
    public ImportNotificationBuilder<T> WithReferenceNumber(ImportNotificationTypeEnum chedType, int scenario,
         DateTime created, int item)
    {
        var prefix = chedType.ConvertToChedType();

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

    public ImportNotificationBuilder<T> WithCommodity(string commodityCode, string description, int netWeight)
    {
        return Do(n =>
        {
            n.PartOne!.Commodities!.TotalNetWeight = netWeight;
            n.PartOne!.Commodities!.TotalGrossWeight = netWeight;
            n.PartOne!.Commodities!.CommodityComplements![0].SpeciesId = "000";
            n.PartOne!.Commodities!.CommodityComplements![0].SpeciesClass = "XXXX";
            n.PartOne!.Commodities!.CommodityComplements![0].SpeciesName = "XXXX";
            n.PartOne!.Commodities!.CommodityComplements![0].CommodityDescription = description;
            n.PartOne!.Commodities!.CommodityComplements![0].ComplementName = "XXXX";
            n.PartOne!.Commodities!.CommodityComplements![0].SpeciesNomination = "XXXX";
            n.PartOne!.Commodities!.ComplementParameterSets![0].SpeciesId = "000";
            n.PartOne!.Commodities!.ComplementParameterSets![0].KeyDataPairs!["netweight"] = netWeight;
        });
    }

    protected override ImportNotificationBuilder<T> Validate()
    {
        return Do(n => { n.ReferenceNumber.AssertHasValue(); });
    }
}