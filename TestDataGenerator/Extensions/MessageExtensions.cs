using Btms.Types.Alvs;
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
            
            default:
                throw new InvalidDataException($"Unexpected type {m.GetType().Name}");
        }
        
    }
}