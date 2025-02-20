using Btms.Model.Ipaffs;
using Btms.Common.Enum;

namespace Btms.Common.Extensions;

public static class ImportNotificationTypeEnumExtensions
{
    public static string AsString(this ImportNotificationTypeEnum? chedType)
    {
        return chedType!.Value.AsString();
    }

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

    public static string FromImportNotificationTypeEnumString(this string s)
    {
        var e = System.Enum.Parse<ImportNotificationTypeEnum>(s);
        return e.AsString();
    }
}