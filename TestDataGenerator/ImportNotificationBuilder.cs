using System.Runtime.Serialization;
using Cdms.Types.Ipaffs;

namespace TestDataGenerator;

public class ImportNotificationBuilder : ImportNotificationBuilder<ImportNotification>
{
    public ImportNotificationBuilder()
    {
    }

    public ImportNotificationBuilder(string file) : base(file)
    {
    }
}

public class ImportNotificationBuilder<T> : BuilderBase<T, ImportNotificationBuilder<T>>
    where T : ImportNotification, new()
{
    public ImportNotificationBuilder() : base()
    {
    }

    public ImportNotificationBuilder(string file) : base(file)
    {
    }

    public static ImportNotificationBuilder<T> Default()
    {
        return new ImportNotificationBuilder<T>();
    }
    
    public static ImportNotificationBuilder<T> FromFile(string file)
    {
        return new ImportNotificationBuilder<T>(file);
    }

    public ImportNotificationBuilder<T> WithRandomCommodities(int min, int max)
    {
        var commodityCount = CreateRandomInt(min, max);

        var commodities = Enumerable.Range(0, commodityCount).Select(x => new CommodityComplement()
            {
                
            }
        );
        
        return Do((n) =>
        {
            n.PartOne!.Commodities.CommodityComplements = commodities.ToArray();
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