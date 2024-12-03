using Btms.Model.Ipaffs;
using Btms.Model.Extensions;

namespace Btms.Model;

public static class ModelHelpers
{
    public static string[] GetChedTypes()
    {
        return Enum.GetValues<ImportNotificationTypeEnum>()
            .Where(t => t != ImportNotificationTypeEnum.Imp)
            .Select(e => e.AsString()).ToArray();
    }
}