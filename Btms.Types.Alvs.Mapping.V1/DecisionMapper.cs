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


using Btms.Model.Cds;

namespace Btms.Types.Alvs.Mapping;

public static class DecisionMapper
{
    public static Btms.Model.Cds.CdsDecision Map(Btms.Types.Alvs.Decision from)
    {
        if (from is null)
        {
            return default!;
        }

        var to = new Btms.Model.Cds.CdsDecision()
        {
            ServiceHeader = ServiceHeaderMapper.Map(from.ServiceHeader!),
            Header = new DecisionHeader()
            {
                DecisionNumber = from.Header!.DecisionNumber,
                EntryReference = from.Header.EntryReference,
                EntryVersionNumber = from.Header.EntryVersionNumber
            },
            Items = from!.Items!.Select(x => new Btms.Model.Cds.DecisionItems()
            {
                ItemNumber = x.ItemNumber,
                Checks = x.Checks!.Select(c => new Btms.Model.Cds.DecisionCheck()
                {
                    CheckCode = c.CheckCode!,
                    DecisionCode = c.DecisionCode!,
                    DecisionReasons = c.DecisionReasons,
                    DecisionsValidUntil = c.DecisionsValidUntil,
                    DepartmentCode = c.DepartmentCode
                })
                .ToArray(),

            }).ToArray()
        };

        return to;
    }
}