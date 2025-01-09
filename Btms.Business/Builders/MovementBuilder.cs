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

public class MovementBuilder(ILogger<MovementBuilder> logger, Movement movement, bool hasChanges = false)
{
    private Movement? _movement = movement;
    public bool HasChanges = hasChanges;

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

        _movement.Items = builder._movement.Items;
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

            if (alvsDecision.Context.DecisionComparison.HasValue())
            {
                //Copy to top level status
                _movement.AlvsDecisionStatus.Context = alvsDecision.Context;
            }

            // if (_movement.AlvsDecisionStatus.Decisions.Count == 0 || ((alvsDecision.Context.AlvsDecisionNumber >=_movement. AlvsDecisionStatus.Context?.AlvsDecisionNumber) &&
            //                                                           (alvsDecision.Context.BtmsDecisionNumber > _movement.AlvsDecisionStatus.Context?.BtmsDecisionNumber)) ||
            //     ((alvsDecision.Context.AlvsDecisionNumber > _movement.AlvsDecisionStatus.Context?.AlvsDecisionNumber) &&
            //      (alvsDecision.Context.BtmsDecisionNumber >= _movement.AlvsDecisionStatus.Context?.BtmsDecisionNumber)))
            // {
            //     _movement.AlvsDecisionStatus.DecisionStatus = alvsDecision.Context.DecisionStatus;
            //     _movement.AlvsDecisionStatus.Context = alvsDecision.Context;
            // }
            // else
            // {
            //     logger.LogWarning("Decision AlvsDecisionNumber {0}, BtmsDecisionNumber {1} received out of sequence, not updating top level status. Top level status currently AlvsDecisionNumber {2}, BtmsDecisionNumber {3}",
            //         alvsDecision.Context.AlvsDecisionNumber, alvsDecision.Context.BtmsDecisionNumber,
            //         _movement.AlvsDecisionStatus.Context?.AlvsDecisionNumber, _movement.AlvsDecisionStatus.Context?.BtmsDecisionNumber);
            // }

            _movement.AlvsDecisionStatus.Decisions.Add(alvsDecision);

            // _movement.AnalyseAlvsStatus();
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

        var alvsDecision = _movement.AlvsDecisionStatus?.Decisions
            .SingleOrDefault(d => d.Context.DecisionComparison.HasValue());
            // .Find(
            // d => d.Context.EntryVersionNumber == _movement.EntryVersionNumber);
            
        if (alvsDecision != null)
        {
            var btmsChecks = _movement
                .Items!
                .SelectMany(i => i.Checks!.Select(c => new { Item = i, Check = c }))
                .ToDictionary(ic => (ic.Item.ItemNumber!.Value, ic.Check.CheckCode!), ic => ic.Check.DecisionCode!);

            var shouldPair = clearanceRequest.Header!.DecisionNumber > alvsDecision.Context.DecisionComparison!.BtmsDecisionNumber;
            
            // Updates the pair status if we've received a newer BTMS decision than that already paired. 
            SetDecisionComparison(alvsDecision, shouldPair, clearanceRequest.Header!.DecisionNumber!.Value);
            
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
            // DecisionComparison = 
            // BtmsDecisionNumber = clearanceRequest.Header!.DecisionNumber!.Value
        };
    }
    
    private AlvsDecision FindBtmsPairAndUpdate(CdsClearanceRequest alvsDecision)
    {
        GuardNullMovement();
        
        var btmsDecisions = _movement.Decisions?
            .Where(d => 
                d.Header!.EntryVersionNumber == _movement.EntryVersionNumber)
            .OrderBy(d => d.Header!.EntryVersionNumber)
            .Reverse();
            
        var mostRecentBtmsDecision = btmsDecisions?.FirstOrDefault();

        var pairedAlvsDecision = _movement
            .AlvsDecisionStatus
            .Decisions
            .SingleOrDefault(d => 
                d.Context.DecisionComparison.HasValue() //&& d.Context.BtmsDecisionNumber == mostRecentBtmsDecision?.Header?.DecisionNumber
            );

        var shouldPair = mostRecentBtmsDecision.HasValue() && (!pairedAlvsDecision.HasValue() ||
                         alvsDecision!.Header!.DecisionNumber
                         > pairedAlvsDecision!.Context.AlvsDecisionNumber);
        
        // var shouldPair = mostRecentBtmsDecision.HasValue();
                         // && (!pairedAlvsDecision.HasValue() || 
                         //     pairedAlvsDecision.Context.BtmsDecisionNumber < clearanceRequest?.Header?.DecisionNumber);

        if (shouldPair && pairedAlvsDecision.HasValue())
        {
            // A BTMS decision should only be paired with a single ALVS decision
            // So if its already paired, remove it.
            SetDecisionComparison(pairedAlvsDecision, false, pairedAlvsDecision!.Context.DecisionComparison!.BtmsDecisionNumber!.Value);
            // pairedAlvsDecision!.Context.Paired = false;
            // pairedAlvsDecision!.Context.BtmsDecisionNumber = null;
        }

        var btmsChecks = mostRecentBtmsDecision ?
            .Items?
            .SelectMany(i => i.Checks!.Select(c => new { Item = i!, Check = c }))
            .ToDictionary(ic => (ic.Item.ItemNumber!.Value, ic.Check.CheckCode!), ic => ic.Check.DecisionCode!);
        
        var alvsDecisionWithContext = new AlvsDecision()
        {
            Decision = alvsDecision!, //TODO : not sure how this can be null...
            Context = new DecisionContext()
            {
                AlvsDecisionNumber = alvsDecision!.Header!.DecisionNumber!,
                EntryVersionNumber = alvsDecision!.Header!.EntryVersionNumber!.Value,
                Checks = alvsDecision
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
        
        SetDecisionComparison(alvsDecisionWithContext, shouldPair, mostRecentBtmsDecision?.Header?.DecisionNumber);
        
        if (shouldPair && mostRecentBtmsDecision.HasValue())
        {
            CompareDecisions(alvsDecisionWithContext, mostRecentBtmsDecision);
        }
        
        return alvsDecisionWithContext;
    }

    private void SetDecisionComparison(AlvsDecision decision, bool paired, int? btmsDecisionNumber)
    {
        logger.LogInformation("SetPairStatus {decision}, {paired} {btmsDecisionNumber}",
            decision.Context.AlvsDecisionNumber, paired, btmsDecisionNumber);
        
        decision.Context.DecisionComparison = !paired ? null : new DecisionComparison()
        {
          Paired  = paired,
          BtmsDecisionNumber = btmsDecisionNumber
        };
        // TODO:
        // decision.Context.BtmsDecisionNumber = paired ? btmsDecisionNumber : null;
        // decision.Context.DecisionStatus
        // decision.Context.DecisionMatched
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

        if (!alvsDecision.Context.DecisionComparison.HasValue())
        {
            throw new InvalidDataException("Should only be comparing when it's been paired");
        }
        
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
        var checksMatch = alvsChecks.All(c => c.AlvsDecisionCode == c.BtmsDecisionCode);
        
        if (checksMatch)
        {
            alvsDecision.Context.DecisionComparison.DecisionMatched = true;
            decisionStatus = DecisionStatusEnum.BtmsMadeSameDecisionAsAlvs;
        }
        // if (_movement.AlvsDecisionStatus.Decisions.All(d => d.Context.DecisionMatched))
        // {
        //     decisionStatus = DecisionStatusEnum.BtmsMadeSameDecisionAsAlvs;
        // }
        else if (!_movement.ClearanceRequests.Exists(c => c.Header!.EntryVersionNumber == 1))
        {
            decisionStatus = DecisionStatusEnum.AlvsClearanceRequestVersion1NotPresent;
        }
        else if (!_movement.ClearanceRequests.AreNumbersComplete(c => c.Header!.EntryVersionNumber!.Value))
        {
            decisionStatus = DecisionStatusEnum.AlvsClearanceRequestVersionsNotComplete;
        }
        else if (!_movement.AlvsDecisionStatus.Decisions.Exists(d => d.Context.AlvsDecisionNumber == 1))
        {
            decisionStatus = DecisionStatusEnum.AlvsDecisionVersion1NotPresent;
        }
        else if (!_movement.AlvsDecisionStatus.Decisions.AreNumbersComplete(d => d.Context.AlvsDecisionNumber))
        {
            decisionStatus = DecisionStatusEnum.AlvsDecisionVersionsNotComplete;
        }
        
        // if (alvsDecision.Context.DecisionComparison.BtmsDecisionNumber == 0)
        // {
        //     decisionStatus = DecisionStatusEnum.BtmsDecisionNotPresent;
        // }
        // else
        // {
        
        // }
        
        alvsDecision.Context.DecisionComparison.DecisionStatus = decisionStatus;
    }

    public Movement Build()
    {
        GuardNullMovement();
        return _movement;
    }
}