using Btms.Common.Extensions;
using Btms.Types.Ipaffs;
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
        return Do(x =>
            x.ReferenceNumber = DataHelpers.GenerateReferenceNumber(chedType, scenario, created, item));
    }

    public ImportNotificationBuilder<T> WithEntryDate(DateTime entryDate)
    {
        return Do(x => x.LastUpdated = entryDate.RandomTime());
    }

    public ImportNotificationBuilder<T> WithRandomArrivalDateTime(int maxDays, int maxHours=6)
    {
        var dayOffset = CreateRandomInt(maxDays * -1, maxDays);
        var hourOffset = CreateRandomInt(maxHours * -1, maxHours);
        
        return Do(x =>
        {
            var dt = DateTime.Today.AddDays(dayOffset).AddHours(hourOffset);
            dt = dt > x.LastUpdated ? dt : x.LastUpdated ?? dt;
            x.PartOne!.ArrivalDate = dt.ToDate();
            x.PartOne!.ArrivalTime = dt.ToTime();
        });
    }

    public ImportNotificationBuilder<T> WithCommodity(string commodityCode, string description, int netWeight)
    {
        return Do(n =>
        {
            if (n.PartOne?.Commodities == null) return;
            n.PartOne.Commodities.TotalNetWeight = netWeight;
            n.PartOne.Commodities.TotalGrossWeight = netWeight;
            var commodityComplement = n.PartOne.Commodities.CommodityComplements?[0];
            if (commodityComplement == null) return;
            commodityComplement.SpeciesId = "000";
            commodityComplement.SpeciesClass = "XXXX";
            commodityComplement.SpeciesName = "XXXX";
            commodityComplement.CommodityDescription = description;
            commodityComplement.ComplementName = "XXXX";
            commodityComplement.SpeciesNomination = "XXXX";
            var complementParameterSet = n.PartOne.Commodities.ComplementParameterSets?[0];
            if (complementParameterSet == null) return;
            complementParameterSet.SpeciesId = "000";
            complementParameterSet.KeyDataPairs!["netweight"] = netWeight;
        });
    }

    protected override ImportNotificationBuilder<T> Validate()
    {
        return Do(n =>
        {
            n.ReferenceNumber.AssertHasValue("Import Notification ReferenceNumber missing");
            n.PartOne!.ArrivalDate.AssertHasValue("Import Notification ArrivalDate missing");
            n.PartOne!.ArrivalTime.AssertHasValue("Import Notification ArrivalTime missing");
        });
    }
}