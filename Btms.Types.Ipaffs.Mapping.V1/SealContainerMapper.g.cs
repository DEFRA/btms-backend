//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
//
//</auto-generated>
//------------------------------------------------------------------------------
#nullable enable


namespace Btms.Types.Ipaffs.Mapping;

public static class SealContainerMapper
{
    public static Btms.Model.Ipaffs.SealContainer Map(Btms.Types.Ipaffs.SealContainer? from)
    {
        if (from is null)
        {
            return default!;
        }
        var to = new Btms.Model.Ipaffs.SealContainer();
        to.SealNumber = from?.SealNumber;
        to.ContainerNumber = from?.ContainerNumber;
        to.OfficialSeal = from?.OfficialSeal;
        to.ResealedSealNumber = from?.ResealedSealNumber;
        return to;
    }
}