using Btms.Types.Ipaffs;

namespace Btms.Types.Ipaffs.V1.Extensions;

public static class ImportNotificationTypeEnumExtensions
{
    public static string AsString(this ImportNotificationTypeEnum chedType)
    {
        return chedType switch
        {
            ImportNotificationTypeEnum.Cveda => "CHEDA",
            ImportNotificationTypeEnum.Cvedp => "CHEDP",
            ImportNotificationTypeEnum.Chedpp => "CHEDPP",
            ImportNotificationTypeEnum.Ced => "CHEDD",
            ImportNotificationTypeEnum.Imp => "IMP",
            _ => throw new ArgumentOutOfRangeException(nameof(chedType), chedType, null)
        };
    }
}