using Btms.Model.Ipaffs;

namespace Btms.Business.Extensions;

public static class ImportNotificationTypeEnumExtensions
{
    public static ImportNotificationTypeEnum? GetChedType(this string documentCode)
    {
        //This is the mapping from https://eaflood.atlassian.net/wiki/spaces/ALVS/pages/5177016349/DocumentCode+Field
        // "C085" isn't on the wiki page, but after a discussion with Matt, it appears it maps to ChedPP
        return documentCode switch
        {
            "9115"or "C633"  or "N002" or "N851" or "C085" => ImportNotificationTypeEnum.Chedpp,
            "N852" or "C678" => ImportNotificationTypeEnum.Ced,
            "C640" => ImportNotificationTypeEnum.Cveda,
            "C641" or "C673" or "N853" => ImportNotificationTypeEnum.Cvedp,
            "9HCG" => null, //TODO : should this be mapped to a ched type?
            _ => null
        };
    }
    
    public static ImportNotificationTypeEnum? GetChedTypeFromCheckCode(this string documentCode)
    {
        //This is the mapping from https://eaflood.atlassian.net/wiki/spaces/ALVS/pages/5400920093/DocumentCode+CheckCode+Mapping
        return documentCode switch
        {
            "H221" => ImportNotificationTypeEnum.Cveda,
            "H223" => ImportNotificationTypeEnum.Ced,
            "H222" or "H224" => ImportNotificationTypeEnum.Cvedp,
            "H219" or "H218" or "H220" => ImportNotificationTypeEnum.Chedpp,
            _ => null
        };
    }
}