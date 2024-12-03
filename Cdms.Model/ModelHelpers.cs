using Cdms.Model.Ipaffs;
using Cdms.Model.Extensions;

namespace Cdms.Model;

public static class ModelHelpers
{
    public static string[] GetChedTypes()
    {
        return Enum.GetValues<ImportNotificationTypeEnum>()
            .Where(t => t != ImportNotificationTypeEnum.Imp)
            .Select(e => e.AsString()).ToArray();
    }
}