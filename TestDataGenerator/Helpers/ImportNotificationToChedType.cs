using Cdms.Types.Ipaffs;

namespace TestDataGenerator.Helpers;

public static class ImportNotificationToChedType
{
    public static string ConvertToChedType(this ImportNotificationTypeEnum chedType)
    {
        return chedType switch
        {
            ImportNotificationTypeEnum.Cveda => "CHEDA",
            ImportNotificationTypeEnum.Cvedp => "CHEDP",
            ImportNotificationTypeEnum.Chedpp => "CHEDPP",
            ImportNotificationTypeEnum.Ced => "CHEDD",
            _ => throw new ArgumentOutOfRangeException(nameof(chedType), chedType, null)
        };
    }
}