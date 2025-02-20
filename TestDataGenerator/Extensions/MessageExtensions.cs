using Btms.Types.Alvs;
using Btms.Types.Gvms;
using Btms.Types.Ipaffs;
using Decision = Btms.Types.Alvs.Decision;

namespace TestDataGenerator.Extensions;

public static class MessageExtensions
{
    public static DateTime CreatedDate(this object m)
    {
        switch (m)
        {
            case AlvsClearanceRequest cr:
                return cr.ServiceHeader!.ServiceCallTimestamp!.Value;

            case ImportNotification n:
                return n.LastUpdated!.Value;

            case Decision d:
                return d.ServiceHeader!.ServiceCallTimestamp!.Value;

            case Finalisation d:
                return d.ServiceHeader!.ServiceCallTimestamp!.Value;
            
            case Gmr d:
                return d.UpdatedSource!.Value;

            case SearchGmrsForDeclarationIdsResponse d:
                return d.Gmrs?.Min(x => x.UpdatedSource) ?? DateTime.MinValue;

            default:
                throw new InvalidDataException($"Unexpected type {m.GetType().Name}");
        }

    }
}