using Cdms.Model.Ipaffs;
using Microsoft.AspNetCore.Components.Forms;

namespace Cdms.Model.Extensions;

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
        var e = Enum.Parse<ImportNotificationTypeEnum>(s);
        return e.AsString();
    }
}