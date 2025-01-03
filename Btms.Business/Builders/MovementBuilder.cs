using System.Diagnostics.CodeAnalysis;
using Btms.Common.Extensions;
using Btms.Model.Auditing;
using Btms.Model.Cds;
using Btms.Model.ChangeLog;
using Microsoft.Extensions.Logging;
using Btms.Business.Extensions;
using Btms.Model;
using Btms.Model.Ipaffs;

namespace Btms.Business.Builders;

public class MovementBuilder(ILogger<MovementBuilder> logger)
{
    private Movement? _movement;
    public bool HasChanges = false;
    
    public MovementBuilder From(Model.Cds.CdsClearanceRequest request)
    {
        logger.LogInformation("Creating movement from clearance request {0}", request.Header!.EntryReference);
        HasChanges = true;
        _movement = new Movement()
        {
            Id = request.Header!.EntryReference,
            UpdatedSource = request.ServiceHeader?.ServiceCalled,
            CreatedSource = request.ServiceHeader?.ServiceCalled,
            ArrivesAt = request.Header.ArrivesAt,
            EntryReference = request.Header.EntryReference!,
            EntryVersionNumber = request.Header.EntryVersionNumber.GetValueOrDefault(),
            MasterUcr = request.Header.MasterUcr!,
            DeclarationType = request.Header.DeclarationType!,
            SubmitterTurn = request.Header.SubmitterTurn!,
            DeclarantId = request.Header.DeclarantId!,
            DeclarantName = request.Header.DeclarantName!,
            DispatchCountryCode = request.Header.DispatchCountryCode!,
            GoodsLocationCode = request.Header.GoodsLocationCode!,
            ClearanceRequests = [request],
            Items = request.Items?.ToList()!,
            BtmsStatus = new MovementStatus()
            {
                ChedTypes = GetChedTypes(request.Items!.ToList()),
                Linked = false,
                LinkStatus = MovementStatus.NotLinkedStatus
            }
        };
        
        return this;
    }

    public MovementBuilder From(Movement movement)
    {
        HasChanges = true;
        _movement = movement;
        return this;
    }

    private ImportNotificationTypeEnum[] GetChedTypes(List<Items>? items = null)
    {
        return items?
            .SelectMany(i => i.Documents!)
            .Select(d =>
                d.DocumentCode!.GetChedType()
            )
            .Distinct()
            .Where(ct => ct.HasValue())
            .Select(ct => ct!.Value)
            .ToArray()!;
    }

    public string Id
    {
        get
        {
            GuardNullMovement();
            return _movement.Id!;
        }
    }

    public bool IsEntryVersionNumberGreaterThan(int? versionNumber)
    {
        GuardNullMovement();
        
        return _movement.ClearanceRequests[^1].Header?.EntryVersionNumber > versionNumber;
    }
    
    public bool IsEntryVersionNumberEqualTo(int? versionNumber)
    {
        GuardNullMovement();
        
        return _movement.ClearanceRequests[^1].Header?.EntryVersionNumber == versionNumber;
    }

    public void ReplaceClearanceRequests(MovementBuilder builder)
    {
        GuardNullMovement();
        builder.GuardNullMovement();
        
        _movement.ClearanceRequests.RemoveAll(x =>
            x.Header?.EntryReference ==
            builder._movement.ClearanceRequests[0].Header?.EntryReference);
        _movement.ClearanceRequests.AddRange(builder._movement.ClearanceRequests);

        _movement.Items.AddRange(builder._movement.Items);
    }

    public ChangeSet GenerateChangeSet(MovementBuilder builder)
    {
        GuardNullMovement();
        builder.GuardNullMovement();
        
        return _movement.ClearanceRequests[^1].GenerateChangeSet(builder._movement.ClearanceRequests[0]);
    }

    public MovementBuilder MergeDecision(string path, Model.Cds.CdsClearanceRequest clearanceRequest, List<DecisionImportNotifications>? notificationContext)
    {
        GuardNullMovement();
        
        HasChanges = true;
        
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
                var existingItem = _movement.Items.Find(x => x.ItemNumber == item.ItemNumber);

                if (existingItem is not null)
                {
                    existingItem.MergeChecks(item);
                }
            }

            context = FindAlvsPairAndUpdate(clearanceRequest);
            
            _movement.Decisions ??= [];
            _movement.Decisions.Add(clearanceRequest);
            
        }
        else if (isAlvs)
        {
            var alvsDecision = FindBtmsPairAndUpdate(clearanceRequest);
            context = alvsDecision.Context;

            if (_movement.AlvsDecisionStatus.Decisions.Count == 0 || ((alvsDecision.Context.AlvsDecisionNumber >=_movement. AlvsDecisionStatus.Context?.AlvsDecisionNumber) &&
                                                                      (alvsDecision.Context.BtmsDecisionNumber > _movement.AlvsDecisionStatus.Context?.BtmsDecisionNumber)) ||
                ((alvsDecision.Context.AlvsDecisionNumber > _movement.AlvsDecisionStatus.Context?.AlvsDecisionNumber) &&
                 (alvsDecision.Context.BtmsDecisionNumber >= _movement.AlvsDecisionStatus.Context?.BtmsDecisionNumber)))
            {
                _movement.AlvsDecisionStatus.DecisionStatus = alvsDecision.Context.DecisionStatus;
                _movement.AlvsDecisionStatus.Context = alvsDecision.Context;
            }
            else
            {
                logger.LogWarning("Decision AlvsDecisionNumber {0}, BtmsDecisionNumber {1} received out of sequence, not updating top level status. Top level status currently AlvsDecisionNumber {2}, BtmsDecisionNumber {3}",
                    alvsDecision.Context.AlvsDecisionNumber, alvsDecision.Context.BtmsDecisionNumber,
                    _movement.AlvsDecisionStatus.Context?.AlvsDecisionNumber, _movement.AlvsDecisionStatus.Context?.BtmsDecisionNumber);
            }

            _movement.AlvsDecisionStatus.Decisions.Add(alvsDecision);

            _movement.AnalyseAlvsStatus();
            _movement.AddLinkStatus();
        }
        else
        {
            throw new ArgumentException(
                $"Unexpected decision source system {clearanceRequest.ServiceHeader?.SourceSystem}");
        }
        
        context.ImportNotifications = notificationContext;
        
        var auditEntry = AuditEntry.CreateDecision(
            BuildNormalizedDecisionPath(path),
            clearanceRequest.Header!.DecisionNumber.GetValueOrDefault(),
            clearanceRequest.ServiceHeader!.ServiceCalled,
            clearanceRequest.Header.DeclarantName!,
            context,
            clearanceRequest.ServiceHeader?.SourceSystem != "BTMS");
        
        this.Update(auditEntry);

        return this;
    }
    
    public void Update(AuditEntry auditEntry)
    {
        GuardNullMovement();
        
        _movement.AuditEntries.Add(auditEntry);
        // matchReferences = [];
    }

    public AuditEntry CreateAuditEntry(string messageId, string source)
    {
        GuardNullMovement();
        
        var auditEntry = AuditEntry.CreateCreatedEntry(
            _movement.ClearanceRequests[0],
            messageId,
            _movement.ClearanceRequests[0].Header?.EntryVersionNumber.GetValueOrDefault() ?? -1,
            _movement.UpdatedSource,
            source);

        return auditEntry;
    }
    
    public AuditEntry UpdateAuditEntry(string messageId, string source, ChangeSet changeSet)
    {
        GuardNullMovement();
        
        var auditEntry = AuditEntry.CreateUpdated(changeSet,
            messageId,
            _movement.ClearanceRequests[0].Header!.EntryVersionNumber.GetValueOrDefault(),
            _movement.UpdatedSource,
            source);

        return auditEntry;
    }

    private static string BuildNormalizedDecisionPath(string fullPath)
    {
        return fullPath.Replace("RAW/DECISIONS/", "");
    }
    
    private DecisionContext FindAlvsPairAndUpdate(CdsClearanceRequest clearanceRequest)
    {
        GuardNullMovement();
        
        // This is an initial implementation
        // we want to be smarter about how we 'pair' things, considering the same version of the import notifications
        // can a BTMS decision be 'paired' to multiple ALVS decisions?
        
        var alvsDecision = _movement.AlvsDecisionStatus?.Decisions.Find(
            d => d.Context.EntryVersionNumber == _movement.EntryVersionNumber);
            
        if (alvsDecision != null)
        {
            var btmsChecks = _movement
                .Items!
                .SelectMany(i => i.Checks!.Select(c => new { Item = i, Check = c }))
                .ToDictionary(ic => (ic.Item.ItemNumber!.Value, ic.Check.CheckCode!), ic => ic.Check.DecisionCode!);
    
            alvsDecision.Context.BtmsDecisionNumber = clearanceRequest.Header!.DecisionNumber!.Value;
            alvsDecision.Context.Paired = true;
            alvsDecision.Context.Checks = alvsDecision
                .Context
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
        GuardNullMovement();
        
        // This is an initial implementation
        // we want to be smarter about how we 'pair' things, considering the same version of the import notifications
        // Q : can a BTMS decision be 'paired' to multiple ALVS decisions? Probably not...
        var btmsDecision = _movement.Decisions?
            .Where(d => 
                d.Header!.EntryVersionNumber == _movement.EntryVersionNumber)
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
                Paired = btmsDecision != null,
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
            }
        };
        
        if (btmsDecision != null)
        {
            CompareDecisions(alvsDecision, btmsDecision);
        }
        
        return alvsDecision;
    }

    [MemberNotNull(nameof(_movement))]
    private void GuardNullMovement()
    {
        if (!_movement.HasValue())
        {
            throw new InvalidOperationException(
                "Can't call this without first calling 'From' to initialise the builder.");
        }
    }
    
    private void CompareDecisions(AlvsDecision alvsDecision, CdsClearanceRequest btmsDecision)
    {
        GuardNullMovement();
        var alvsChecks = alvsDecision.Context.Checks;
        var btmsChecks = alvsDecision.Context.Checks;

        alvsDecision.Context.AlvsCheckStatus = new StatusChecker()
        {
            AllMatch = alvsChecks.All(c => !c.AlvsDecisionCode.StartsWith('X')),
            AnyMatch = alvsChecks.Any(c => !c.AlvsDecisionCode.StartsWith('X')),
            AllNoMatch = alvsChecks.All(c => c.AlvsDecisionCode.StartsWith('X')),
            AnyNoMatch = alvsChecks.Any(c => c.AlvsDecisionCode.StartsWith('X')),
            AllRefuse = alvsChecks.All(c => c.AlvsDecisionCode.StartsWith('N')),
            AnyRefuse = alvsChecks.Any(c => c.AlvsDecisionCode.StartsWith('N')),
            AllRelease = alvsChecks.All(c => c.AlvsDecisionCode.StartsWith('C')),
            AnyRelease = alvsChecks.Any(c => c.AlvsDecisionCode.StartsWith('C')),
            AllHold = alvsChecks.All(c => c.AlvsDecisionCode.StartsWith('H')),
            AnyHold = alvsChecks.Any(c => c.AlvsDecisionCode.StartsWith('H'))
        };
        
        
        alvsDecision.Context.BtmsCheckStatus = new StatusChecker()
        {
            AllMatch = btmsChecks.All(c => c.BtmsDecisionCode.HasValue() && c.BtmsDecisionCode.StartsWith('X')),
            AnyMatch = btmsChecks.Any(c => !(c.BtmsDecisionCode.HasValue() && c.BtmsDecisionCode.StartsWith('X'))),
            AllNoMatch = btmsChecks.All(c => c.BtmsDecisionCode.HasValue() && c.BtmsDecisionCode.StartsWith('X')),
            AnyNoMatch = btmsChecks.Any(c => c.BtmsDecisionCode.HasValue() && c.BtmsDecisionCode.StartsWith('X')),
            AllRefuse = btmsChecks.All(c => c.BtmsDecisionCode.HasValue() && c.BtmsDecisionCode.StartsWith('N')),
            AnyRefuse = btmsChecks.Any(c => c.BtmsDecisionCode.HasValue() && c.BtmsDecisionCode.StartsWith('N')),
            AllRelease = btmsChecks.All(c => c.BtmsDecisionCode.HasValue() && c.BtmsDecisionCode.StartsWith('C')),
            AnyRelease = btmsChecks.Any(c => c.BtmsDecisionCode.HasValue() && c.BtmsDecisionCode.StartsWith('C')),
            AllHold = btmsChecks.All(c => c.BtmsDecisionCode.HasValue() && c.BtmsDecisionCode.StartsWith('H')),
            AnyHold = btmsChecks.Any(c => c.BtmsDecisionCode.HasValue() && c.BtmsDecisionCode.StartsWith('H'))
        };
        
        var decisionStatus = DecisionStatusEnum.InvestigationNeeded;
        
        if (alvsDecision.Context.BtmsDecisionNumber == 0)
        {
            decisionStatus = DecisionStatusEnum.BtmsDecisionNotPresent;
        }
        else
        {
            var checksMatch = alvsChecks.All(c => c.AlvsDecisionCode == c.BtmsDecisionCode);
            
            if (checksMatch)
            {
                alvsDecision.Context.DecisionMatched = true;
                decisionStatus = DecisionStatusEnum.BtmsMadeSameDecisionAsAlvs;
            }
        }
        
        alvsDecision.Context.DecisionStatus = decisionStatus;
    }

    public Movement Build()
    {
        GuardNullMovement();
        return _movement;
    }
}