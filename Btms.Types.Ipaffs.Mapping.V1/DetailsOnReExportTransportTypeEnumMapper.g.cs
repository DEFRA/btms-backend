//------------------------------------------------------------------------------
// <auto-generated>
    //     This code was generated from a template.
    //
    //     Manual changes to this file may cause unexpected behavior in your application.
    //     Manual changes to this file will be overwritten if the code is regenerated.
    // </auto-generated>
//------------------------------------------------------------------------------
#nullable enable
namespace Btms.Types.Ipaffs.Mapping;

public static class DetailsOnReExportTransportTypeEnumMapper
{
    public static Btms.Model.Ipaffs.DetailsOnReExportTransportTypeEnum? Map(
        Btms.Types.Ipaffs.DetailsOnReExportTransportTypeEnum? from)
    {
        if (from == null)
        {
            return default!;
        }

        return from switch
        {
            Btms.Types.Ipaffs.DetailsOnReExportTransportTypeEnum.Rail => Btms.Model.Ipaffs
                .DetailsOnReExportTransportTypeEnum.Rail,
            Btms.Types.Ipaffs.DetailsOnReExportTransportTypeEnum.Plane => Btms.Model.Ipaffs
                .DetailsOnReExportTransportTypeEnum.Plane,
            Btms.Types.Ipaffs.DetailsOnReExportTransportTypeEnum.Ship => Btms.Model.Ipaffs
                .DetailsOnReExportTransportTypeEnum.Ship,
            Btms.Types.Ipaffs.DetailsOnReExportTransportTypeEnum.Road => Btms.Model.Ipaffs
                .DetailsOnReExportTransportTypeEnum.Road,
            Btms.Types.Ipaffs.DetailsOnReExportTransportTypeEnum.Other => Btms.Model.Ipaffs
                .DetailsOnReExportTransportTypeEnum.Other,
            Btms.Types.Ipaffs.DetailsOnReExportTransportTypeEnum.CShipRoad => Btms.Model.Ipaffs
                .DetailsOnReExportTransportTypeEnum.CShipRoad,
            Btms.Types.Ipaffs.DetailsOnReExportTransportTypeEnum.CShipRail => Btms.Model.Ipaffs
                .DetailsOnReExportTransportTypeEnum.CShipRail,

            _ => throw new ArgumentOutOfRangeException(nameof(from), from, null)
        };
    }


}


