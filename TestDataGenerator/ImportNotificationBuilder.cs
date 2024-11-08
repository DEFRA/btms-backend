using System.Runtime.Serialization;
using Cdms.Types.Ipaffs;

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
    
    protected ImportNotificationBuilder<T> WithClean()
    {
        // TODO : This was needed temporarily due to an issue in serialisation
        // Do(n => Array.ForEach(n.PartOne!.Commodities!.ComplementParameterSets!, x => x.KeyDataPairs = null));
        
        return this; 
    }

    public ImportNotificationBuilder<T> WithRandomCommodities(int min, int max)
    {
        var commodityCount = CreateRandomInt(min, max);

        return Do((n) =>
        {
            var commodities = Enumerable.Range(0, commodityCount)
                .Select(x => n.PartOne!.Commodities!.CommodityComplements![0]
                ).ToArray();
            
            n.PartOne!.Commodities!.CommodityComplements = commodities;
        });
    }

    public ImportNotificationBuilder<T> WithReferenceNumber(ImportNotificationTypeEnum chedType, int item)
    {
        var prefix = chedType switch
        {
            ImportNotificationTypeEnum.Cveda => "CHEDA",
            ImportNotificationTypeEnum.Cvedp => "CHEDP",
            ImportNotificationTypeEnum.Chedpp => "CHEDPP",
            ImportNotificationTypeEnum.Ced => "CHEDD",
            _ => throw new ArgumentOutOfRangeException(nameof(chedType), chedType, null),
        };

        // TODO : We may need a way to guarantee these don't collide?....
        return With(x => x.ReferenceNumber, $"{prefix}.GB.{DateTime.Now.Year}.{CreateRandomInt(7)}");
    }

    public ImportNotificationBuilder<T> WithEntryDate(DateTime entryDate)
    {
        return With(x => x.LastUpdated, entryDate);
    }

    // public ImportNotificationBuilder<T> WithStatus(Status status)
    // {
    //     var field = typeof(Status).GetField(status.ToString());
    //     var description = field
    //         ?.GetCustomAttributes(typeof(EnumMemberAttribute), false)
    //         .Cast<EnumMemberAttribute>()
    //         .Select(x => x.Value)
    //         .FirstOrDefault();
    //
    //     return With(x => x.Status, description ?? status.ToString("G"));
    // }
}