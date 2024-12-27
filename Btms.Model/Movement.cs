using Btms.Model.Extensions;
using JsonApiDotNetCore.MongoDb.Resources;
using JsonApiDotNetCore.Resources.Annotations;
using MongoDB.Bson.Serialization.Attributes;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Btms.Common.Extensions;
using Btms.Model.Cds;
using Btms.Model.Auditing;
using Btms.Model.ChangeLog;
using Btms.Model.Data;
using Btms.Model.Relationships;
using Microsoft.Extensions.Logging;

namespace Btms.Model;

// Recreation of ClearanceRequest schema from
// https://eaflood.atlassian.net/wiki/spaces/TRADE/pages/5104664583/PHA+Port+Health+Authority+Integration+Data+Schema

[Resource]
public class Movement : IMongoIdentifiable, IDataEntity, IAuditable
{
    private List<string> matchReferences = [];

    // This field is used by the jsonapi-consumer to control the correct casing in the type field
    [ChangeSetIgnore]
    public string Type { get; set; } = "movements";

    [Attr] public List<CdsClearanceRequest> ClearanceRequests { get; set; } = default!;

    [Attr] public List<CdsClearanceRequest> Decisions { get; set; } = default!;

    [Attr] public AlvsDecisionStatus AlvsDecisionStatus { get; set; } = new AlvsDecisionStatus();
    
    [Attr] public List<Items> Items { get; set; } = [];

    [Attr]
    public DateTime? UpdatedSource { get; set; }
    [Attr] public DateTime? CreatedSource { get; set; }

    [Attr] public string EntryReference { get; set; } = default!;

    [Attr] public int EntryVersionNumber { get; set; } = default!;

    [Attr] public string MasterUcr { get; set; } = default!;

    [Attr] public int? DeclarationPartNumber { get; set; }

    [Attr] public string DeclarationType { get; set; } = default!;

    [Attr] public DateTime? ArrivesAt { get; set; }

    [Attr] public string SubmitterTurn { get; set; } = default!;

    [Attr] public string DeclarantId { get; set; } = default!;

    [Attr] public string DeclarantName { get; set; } = default!;

    [Attr] public string DispatchCountryCode { get; set; } = default!;

    [Attr] public string GoodsLocationCode { get; set; } = default!;

    [Attr] 
    [ChangeSetIgnore]
    public List<AuditEntry> AuditEntries { get; set; } = new List<AuditEntry>();

    [Attr]
    [JsonPropertyName("relationships")]
    public MovementTdmRelationships Relationships { get; set; } = new MovementTdmRelationships();


    [BsonElement("_matchReferences")]
    [ChangeSetIgnore]
    public List<string> _MatchReferences
    {
        get
        {
            if (!matchReferences.Any())
            {
                var list = new HashSet<string>();

                foreach (var identifier in Items.SelectMany(item => item.GetIdentifiers()))
                {
                    list.Add(identifier);
                }

                matchReferences = list.ToList();
            }

            return matchReferences;
           
        }
        set => matchReferences = value;
    }

    public void AddRelationship(TdmRelationshipObject relationship)
    {
        bool linked = false;
        Relationships.Notifications.Links ??= relationship.Links;

        var dataItems = relationship.Data.Where(dataItem =>
            Relationships.Notifications.Data.TrueForAll(x => x.Id != dataItem.Id))
            .ToList();

        if (dataItems.Any())
        {
            Relationships.Notifications.Data.AddRange(dataItems);
            linked = true;
        }

        Relationships.Notifications.Matched = Items
            .Select(x => x.ItemNumber)
            .All(itemNumber =>
                Relationships.Notifications.Data.Exists(x => x.Matched.GetValueOrDefault() && x.SourceItem == itemNumber));

        if (linked)
        {
            AuditEntries.Add(AuditEntry.CreateLinked(String.Empty, this.AuditEntries.FirstOrDefault()?.Version ?? 1));
        }
    }

    public void Update(AuditEntry auditEntry)
    {
        this.AuditEntries.Add(auditEntry);
        matchReferences = [];
    }

    private DecisionContext FindAlvsPairAndUpdate(CdsClearanceRequest clearanceRequest)
    {
        // This is an initial implementation
        // we want to be smarter about how we 'pair' things, considering the same version of the import notifications
        // can a BTMS decision be 'paired' to multiple ALVS decisions?
        
        var alvsDecision = this.AlvsDecisionStatus?.Decisions.Find(
            d => d.Context.EntryVersionNumber == EntryVersionNumber);
            
        if (alvsDecision != null)
        {
            var btmsChecks = this
                .Items!
                .SelectMany(i => i.Checks!.Select(c => new { Item = i, Check = c }))
                .ToDictionary(ic => (ic.Item.ItemNumber!.Value, ic.Check.CheckCode!), ic => ic.Check.DecisionCode!);

            alvsDecision.Context.BtmsDecisionNumber = clearanceRequest.Header!.DecisionNumber!.Value;
            alvsDecision.Context.Paired = true;
            alvsDecision.Checks = alvsDecision
                .Checks
                .Select(c =>
                {
                    var decisionCode = btmsChecks[(c.ItemNumber, c.CheckCode)];
                    c.BtmsDecisionCode = decisionCode;
                    return c;
                }).ToList();

            CompareDecisions(alvsDecision, clearanceRequest);

            return alvsDecision.Context;
        }
        
        // I'm Sure there's a better way to do this!
        return new DecisionContext()
        {
            EntryVersionNumber = clearanceRequest.Header!.EntryVersionNumber!.Value,
            BtmsDecisionNumber = clearanceRequest.Header!.DecisionNumber!.Value
        };
    }
    
    private AlvsDecision FindBtmsPairAndUpdate(CdsClearanceRequest clearanceRequest)
    {
        // This is an initial implementation
        // we want to be smarter about how we 'pair' things, considering the same version of the import notifications
        // can a BTMS decision be 'paired' to multiple ALVS decisions?
        var btmsDecision = this.Decisions?
            .Where(d => 
                d.Header!.EntryVersionNumber == EntryVersionNumber)
            .OrderBy(d => d.ServiceHeader!.ServiceCalled)
            .Reverse()
            .FirstOrDefault();

        var btmsChecks = btmsDecision ?
            .Items!
            .SelectMany(i => i.Checks!.Select(c => new { Item = i!, Check = c }))
            .ToDictionary(ic => (ic.Item.ItemNumber!.Value, ic.Check.CheckCode!), ic => ic.Check.DecisionCode!);
        
        var alvsDecision = new AlvsDecision()
        {
            Decision = clearanceRequest,
            Context = new DecisionContext()
            {
                AlvsDecisionNumber = clearanceRequest!.Header!.DecisionNumber!.Value,
                BtmsDecisionNumber = btmsDecision == null ? 0 : btmsDecision!.Header!.DecisionNumber!.Value,
                EntryVersionNumber = clearanceRequest!.Header!.EntryVersionNumber!.Value,
                Paired = btmsDecision != null
            },
            Checks = clearanceRequest
                .Items!.SelectMany(i => i.Checks!.Select(c => new { Item = i, Check = c }))
                .Select(ic =>
                {
                    var decisionCode = btmsChecks == null ? null : btmsChecks!.GetValueOrDefault((ic.Item.ItemNumber!.Value, ic.Check.CheckCode!), null);
                    return new ItemCheck()
                    {
                        ItemNumber = ic.Item!.ItemNumber!.Value,
                        CheckCode = ic.Check!.CheckCode!,
                        AlvsDecisionCode = ic.Check!.DecisionCode!,
                        BtmsDecisionCode = decisionCode
                    };
                })
                .ToList()
        };
        
        if (btmsDecision != null)
        {
            CompareDecisions(alvsDecision, btmsDecision);
        }
        
        return alvsDecision;
    }
    
    private void CompareDecisions(AlvsDecision alvsDecision, CdsClearanceRequest btmsDecision)
    {
        alvsDecision.Context.AlvsAllMatch = alvsDecision.Checks.All(c => !c.AlvsDecisionCode.StartsWith('X'));
        alvsDecision.Context.AlvsAnyMatch = alvsDecision.Checks.Any(c => !c.AlvsDecisionCode.StartsWith('X'));
        alvsDecision.Context.AlvsAllNoMatch = alvsDecision.Checks.All(c => c.AlvsDecisionCode.StartsWith('X'));
        alvsDecision.Context.AlvsAnyNoMatch = alvsDecision.Checks.Any(c => c.AlvsDecisionCode.StartsWith('X'));
        alvsDecision.Context.AlvsAllRefuse = alvsDecision.Checks.All(c => c.AlvsDecisionCode.StartsWith('N'));
        alvsDecision.Context.AlvsAnyRefuse = alvsDecision.Checks.Any(c => c.AlvsDecisionCode.StartsWith('N'));
        alvsDecision.Context.AlvsAllRelease = alvsDecision.Checks.All(c => c.AlvsDecisionCode.StartsWith('C'));
        alvsDecision.Context.AlvsAnyRelease = alvsDecision.Checks.Any(c => c.AlvsDecisionCode.StartsWith('C'));
        alvsDecision.Context.AlvsAllHold = alvsDecision.Checks.All(c => c.AlvsDecisionCode.StartsWith('H'));
        alvsDecision.Context.AlvsAnyHold = alvsDecision.Checks.Any(c => c.AlvsDecisionCode.StartsWith('H'));

        alvsDecision.Context.BtmsAllMatch = alvsDecision.Checks.All(
            c => c.BtmsDecisionCode != null && !c.BtmsDecisionCode.StartsWith('X'));
        alvsDecision.Context.BtmsAnyMatch = alvsDecision.Checks.Any(
            c => c.BtmsDecisionCode != null && !c.BtmsDecisionCode.StartsWith('X'));
        alvsDecision.Context.BtmsAllNoMatch = alvsDecision.Checks.All(
            c => c.BtmsDecisionCode != null && c.BtmsDecisionCode.StartsWith('X'));
        alvsDecision.Context.BtmsAnyNoMatch = alvsDecision.Checks.Any(
            c => c.BtmsDecisionCode != null && c.BtmsDecisionCode.StartsWith('X'));
        alvsDecision.Context.BtmsAllRefuse = alvsDecision.Checks.All(
            c => c.BtmsDecisionCode != null && c.BtmsDecisionCode.StartsWith('N'));
        alvsDecision.Context.BtmsAnyRefuse = alvsDecision.Checks.Any(
            c => c.BtmsDecisionCode != null && c.BtmsDecisionCode.StartsWith('N'));
        alvsDecision.Context.BtmsAllRelease = alvsDecision.Checks.All(
            c => c.BtmsDecisionCode != null && c.BtmsDecisionCode.StartsWith('C'));
        alvsDecision.Context.BtmsAnyRelease = alvsDecision.Checks.Any(
            c => c.BtmsDecisionCode != null && c.BtmsDecisionCode.StartsWith('C'));
        alvsDecision.Context.BtmsAllHold = alvsDecision.Checks.All(
            c => c.BtmsDecisionCode != null && c.BtmsDecisionCode.StartsWith('H'));
        alvsDecision.Context.BtmsAnyHold = alvsDecision.Checks.Any(
            c => c.BtmsDecisionCode != null && c.BtmsDecisionCode.StartsWith('H'));

        var pairStatus = "Investigation Needed";
        
        if (alvsDecision.Context.BtmsDecisionNumber == 0)
        {
            pairStatus = "Btms Decision Not Present";
        }
        else
        {
            var checksMatch = alvsDecision.Checks.All(c => c.AlvsDecisionCode == c.BtmsDecisionCode);
            
            if (checksMatch)
            {
                alvsDecision.Context.DecisionMatched = true;
                pairStatus = "Btms Made Same Decision As Alvs";
            }
            else if (!this.ClearanceRequests.Exists(c => c.Header!.EntryVersionNumber == 1))
            {
                pairStatus = "Alvs Clearance Request Version 1 Not Present";
            }
            else if (alvsDecision.Decision.Header!.DecisionNumber != 1 && (this.AlvsDecisionStatus == null || !this.AlvsDecisionStatus.Decisions.Exists(c => c.Context.AlvsDecisionNumber == 1)))
            {
                pairStatus = "Alvs Decision Version 1 Not Present";
            }
        }
        
        alvsDecision.Context.DecisionStatus = pairStatus;
    }

    public bool MergeDecision(string path, CdsClearanceRequest clearanceRequest)
    {
        var sourceSystem = clearanceRequest.ServiceHeader?.SourceSystem;
        var isAlvs = sourceSystem != "BTMS";
        var isBtms = sourceSystem == "BTMS";
        // CdsClearanceRequest? btmsDecision = null;
        // AlvsDecision? alvsDecision = null;
        DecisionContext context;
        
        if (isBtms)
        {
            foreach (var item in clearanceRequest.Items!)
            {
                var existingItem = this.Items.Find(x => x.ItemNumber == item.ItemNumber);

                if (existingItem is not null)
                {
                    existingItem.MergeChecks(item);
                }
            }

            context = FindAlvsPairAndUpdate(clearanceRequest);
            
            Decisions ??= [];
            Decisions.Add(clearanceRequest);
            
        }
        else if (isAlvs)
        {
            var alvsDecision = FindBtmsPairAndUpdate(clearanceRequest);
            context = alvsDecision.Context;

            if (AlvsDecisionStatus.Decisions.Count == 0 || ((alvsDecision.Context.AlvsDecisionNumber >= AlvsDecisionStatus.Context?.AlvsDecisionNumber) &&
                     (alvsDecision.Context.BtmsDecisionNumber > AlvsDecisionStatus.Context?.BtmsDecisionNumber)) ||
                    ((alvsDecision.Context.AlvsDecisionNumber > AlvsDecisionStatus.Context?.AlvsDecisionNumber) &&
                     (alvsDecision.Context.BtmsDecisionNumber >= AlvsDecisionStatus.Context?.BtmsDecisionNumber)))
            {
                AlvsDecisionStatus.DecisionStatus = alvsDecision.Context.DecisionStatus!;
                AlvsDecisionStatus.Context = alvsDecision.Context;
                AlvsDecisionStatus.Checks = alvsDecision.Checks;
            }
            // else
            // {
            //     logger.LogWarning("Decision AlvsDecisionNumber {0}, BtmsDecisionNumber {1} received out of sequence, not updating top level status. Top level status currently AlvsDecisionNumber {2}, BtmsDecisionNumber {3}",
            //         alvsDecision.Context.AlvsDecisionNumber, alvsDecision.Context.BtmsDecisionNumber,
            //         AlvsDecisionStatus.Context?.AlvsDecisionNumber, AlvsDecisionStatus.Context?.BtmsDecisionNumber);
            // }
                
            AlvsDecisionStatus.Decisions.Add(alvsDecision);
            // }
            // else 
            // {
            //     AlvsDecisionStatus = new AlvsDecisionStatus()
            //     {
            //         Decisions = [alvsDecision],
            //         DecisionStatus = alvsDecision.Context.DecisionStatus!,
            //         Context = alvsDecision.Context,
            //         Checks = alvsDecision.Checks
            //     };
            // }
            
            // AlvsDecisions ??= [];
            // AlvsDecisionStatus?.Decisions.Add(alvsDecision);
        }
        else
        {
            throw new ArgumentException(
                $"Unexpected decision source system {clearanceRequest.ServiceHeader?.SourceSystem}");
        }

        // var decisionAuditContext = new Dictionary<string, Dictionary<string, string>>();
        // decisionAuditContext.Add("clearanceRequests", new Dictionary<string, string>()
        // {
        //     { clearanceRequest.Header!.EntryReference!, clearanceRequest.Header!.EntryVersionNumber!.ToString()! }
        // });
        // decisionAuditContext.Add("decisions", new Dictionary<string, string>()
        // {
        //     { clearanceRequest.Header!.EntryReference!, clearanceRequest.Header!.DecisionNumber!.ToString()! }
        // });
        // // decisionAuditContext.Add("importNotifications", new Dictionary<string, string>()
        // // {
        // //     { "todo", "todo" }
        // // });
        //
        var auditEntry = AuditEntry.CreateDecision(
            BuildNormalizedDecisionPath(path),
            clearanceRequest.Header!.DecisionNumber.GetValueOrDefault(),
            clearanceRequest.ServiceHeader!.ServiceCalled,
            clearanceRequest.Header.DeclarantName!,
            context,
            clearanceRequest.ServiceHeader?.SourceSystem != "BTMS");
        this.Update(auditEntry);

        return true;
    }

    private static string BuildNormalizedDecisionPath(string fullPath)
    {
        return fullPath.Replace("RAW/DECISIONS/", "");
    }

    [BsonIgnore]
    [NotMapped]
    [ChangeSetIgnore]
    public string? StringId
    {
        get => Id;
        set => Id = value;
    }

    [BsonIgnore]
    [NotMapped]
    [Attr]
    public string? LocalId { get; set; }

    [Attr]
    [BsonId]
    public string? Id { get; set; } = null!;

    [ChangeSetIgnore]
    public string _Etag { get; set; } = null!;

    [Attr]
    [ChangeSetIgnore]
    public DateTime Created { get; set; }

    [Attr]
    [ChangeSetIgnore] 
    public DateTime Updated { get; set; }
    public AuditEntry GetLatestAuditEntry()
    {
        return this.AuditEntries.OrderByDescending(x => x.CreatedLocal).First();
    }
}