/*------------------------------------------------------------------------------
<auto-generated>
    This code was generated from the EnumMapper template.
    Manual changes to this file may cause unexpected behavior in your application.
    Manual changes to this file will be overwritten if the code is regenerated.
</auto-generated>
------------------------------------------------------------------------------*/
#nullable enable
namespace Btms.Types.Gvms.Mapping;

public static class DirectionEnumMapper
{
    public static Btms.Model.Gvms.DirectionEnum? Map(Btms.Types.Gvms.DirectionEnum? from)
    {
        if (from == default) return default;

        return from switch
        {
            Btms.Types.Gvms.DirectionEnum.UkInbound => Btms.Model.Gvms.DirectionEnum.UkInbound,
            Btms.Types.Gvms.DirectionEnum.UkOutbound => Btms.Model.Gvms.DirectionEnum.UkOutbound,
            Btms.Types.Gvms.DirectionEnum.GbToNi => Btms.Model.Gvms.DirectionEnum.GbToNi,
            Btms.Types.Gvms.DirectionEnum.NiToGb => Btms.Model.Gvms.DirectionEnum.NiToGb,
            _ => throw new ArgumentOutOfRangeException(nameof(from), from, "Unable to map enum DirectionEnum value Btms.Types.Gvms.DirectionEnum")
        };
    }
}
