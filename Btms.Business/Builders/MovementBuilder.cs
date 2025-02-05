using System.Diagnostics.CodeAnalysis;
using Btms.Business.Extensions;
using Btms.Common.Extensions;
using Btms.Model;
using Btms.Model.Auditing;
using Btms.Model.Cds;
using Btms.Model.ChangeLog;
using Btms.Model.Ipaffs;
using Microsoft.Extensions.Logging;

namespace Btms.Business.Builders;

public class MovementBuilder(ILogger<MovementBuilder> logger, DecisionStatusFinder decisionStatusFinder, Movement movement, bool hasChanges = false)
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
        _movement._MatchReferences = builder._movement._MatchReferences;
    }

    public ChangeSet GenerateChangeSet(MovementBuilder builder)
    {
        GuardNullMovement();
        builder.GuardNullMovement();
        
        return _movement.GenerateChangeSet(builder._movement);
    }

    public MovementBuilder MergeFinalisation(string path, CdsFinalisation finalisation)
    {
        GuardNullMovement();

        if (!_movement.FinalisedSource.HasValue() || finalisation.ServiceHeader!.ServiceCalled > _movement.FinalisedSource)
        {
            HasChanges = true;

            _movement.FinalisedSource = finalisation.ServiceHeader!.ServiceCalled;
            _movement.Finalised = DateTime.Now;
            _movement.Finalisation = new Finalisation()
            {
                FinalState = finalisation.Header.FinalState, ManualAction = finalisation.Header.ManualAction
            };
            
            var auditEntry = AuditEntry.CreateFinalisation(
                BuildNormalizedDecisionPath(path),
                finalisation.Header!.DecisionNumber.GetValueOrDefault(),
                finalisation.ServiceHeader!.ServiceCalled,
                finalisation);
        
            this.Update(auditEntry);
        }
        else
        {
            logger.LogInformation("Finalisation message discarded");
            
            var auditEntry = AuditEntry.CreateFinalisation(
                BuildNormalizedDecisionPath(path),
                finalisation.Header!.DecisionNumber.GetValueOrDefault(),
                finalisation.ServiceHeader!.ServiceCalled,
                finalisation, true);
        
            this.Update(auditEntry);
        }
        
        return this;
    }

    public MovementBuilder MergeDecision(string path, CdsDecision decision, List<DecisionImportNotifications>? notificationContext)
    {
        GuardNullMovement();
        
        HasChanges = true;
        
        var sourceSystem = decision.ServiceHeader?.SourceSystem;
        var isAlvs = sourceSystem == "ALVS";
        var isBtms = sourceSystem == "BTMS";
        DecisionContext context;
        
        if (isBtms)
        {
            foreach (var item in decision.Items!)
            {
                var existingItem = _movement.Items.Find(x => x.ItemNumber == item.ItemNumber);

                if (existingItem is not null)
                {
                    existingItem.MergeChecks(item);
                }
            }

            _movement.Decisions ??= [];
            _movement.Decisions.Add(decision);
            
            context = FindAlvsPairAndUpdate(decision);
        }
        else if (isAlvs)
        {
            var alvsDecision = CreateAlvsDecisionWithContext(decision);
            context = alvsDecision.Context;
            
            _movement.AlvsDecisionStatus.Decisions.Add(alvsDecision);

            FindBtmsPairAndCompare(alvsDecision);
                
            if (alvsDecision.Context.DecisionComparison.HasValue())
            {
                //Copy to top level status
                _movement.AlvsDecisionStatus.Context = alvsDecision.Context;
            }
        }
        else
        {
            throw new ArgumentException(
                $"Unexpected decision source system {decision.ServiceHeader?.SourceSystem}");
        }
        
        context.ImportNotifications = notificationContext;
        
        var auditEntry = AuditEntry.CreateDecision(
            BuildNormalizedDecisionPath(path),
            decision.Header!.DecisionNumber.GetValueOrDefault(),
            decision.ServiceHeader!.ServiceCalled,
            context,
            decision.ServiceHeader?.SourceSystem != "BTMS");
        
        this.Update(auditEntry);

        return this;
    }
    
    public void Update(AuditEntry auditEntry)
    {
        GuardNullMovement();
        
        _movement.AuditEntries.Add(auditEntry);
        _movement.Updated = DateTime.UtcNow;
    }

    public AuditEntry CreateAuditEntry(string messageId, CreatedBySystem source)
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
    
    public AuditEntry UpdateAuditEntry(string messageId, CreatedBySystem source, ChangeSet changeSet)
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
    
    private DecisionContext FindAlvsPairAndUpdate(CdsDecision btmsDecision)
    {
        GuardNullMovement();
        
        var alvsDecision = _movement.AlvsDecisionStatus.Decisions
            .MaxBy(d => d.Context.DecisionComparison.HasValue());
            
        logger.LogInformation("FindAlvsPairAndUpdate btmsDecision number {BtmsDecisionNumber}, alvs paired decision number {PairedDecisionNumber}", 
            btmsDecision.Header!.DecisionNumber, alvsDecision?.Context.DecisionComparison?.BtmsDecisionNumber);
        
        if (alvsDecision != null)
        {
            var shouldPair = btmsDecision.Header!.DecisionNumber > alvsDecision.Context.DecisionComparison?.BtmsDecisionNumber;
            
            logger.LogInformation("FindAlvsPairAndUpdate ShouldPair {ShouldPair}", shouldPair);

            // Updates the pair status if we've received a newer BTMS decision than that already paired. 
            SetDecisionComparison(alvsDecision, shouldPair, btmsDecision.Header.DecisionNumber);

            if (shouldPair)
            {
                CompareDecisions(alvsDecision, btmsDecision);    
            }

            _movement.AlvsDecisionStatus.Context = alvsDecision.Context;
            
            return alvsDecision.Context;
        }
        
        return new DecisionContext()
        {
            EntryVersionNumber = btmsDecision.Header!.EntryVersionNumber!.Value,
        };
    }

    private void FindBtmsPairAndCompare(AlvsDecision alvsDecision)
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
                    d.Context.DecisionComparison
                        .HasValue()
            );

        var shouldPair = mostRecentBtmsDecision.HasValue() && (!pairedAlvsDecision.HasValue() ||
                                                               alvsDecision!.Decision.Header!.DecisionNumber
                                                               > pairedAlvsDecision!.Context.AlvsDecisionNumber);
        
        if (shouldPair && pairedAlvsDecision.HasValue())
        {
            // A BTMS decision should only be paired with a single ALVS decision
            // So if its already paired, remove it.
            SetDecisionComparison(pairedAlvsDecision, false,
                pairedAlvsDecision!.Context.DecisionComparison!.BtmsDecisionNumber!.Value);
        }
        
        SetDecisionComparison(alvsDecision, shouldPair, mostRecentBtmsDecision?.Header?.DecisionNumber);
        
        if (shouldPair && mostRecentBtmsDecision.HasValue())
        {
            CompareDecisions(alvsDecision, mostRecentBtmsDecision);
        }
    }
    

    private AlvsDecision CreateAlvsDecisionWithContext(CdsDecision alvsDecision)
    {
        GuardNullMovement();
        
        var alvsDecisionWithContext = new AlvsDecision()
        {
            Decision = alvsDecision!, //TODO : not sure how this can be null...
            Context = new DecisionContext()
            {
                AlvsDecisionNumber = alvsDecision!.Header!.DecisionNumber!,
                EntryVersionNumber = alvsDecision!.Header!.EntryVersionNumber!.Value
            }
        };
        
        return alvsDecisionWithContext;
    }

    private void SetDecisionComparison(AlvsDecision decision, bool paired, int? btmsDecisionNumber)
    {
        logger.LogInformation("SetDecisionComparison SetPairStatus AlvsDecision {AlvsDecision}, Paired {Paired} BtmsDecisionNumber {BtmsDecisionNumber}",
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

    private StatusChecker GetAlvsCheckStatus(List<ItemCheck> alvsChecks)
    {
        return  new StatusChecker()
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
    }

    private StatusChecker GeBtmsCheckStatus(List<ItemCheck> alvsChecks)
    {
        return new StatusChecker()
        {
            AllMatch = alvsChecks.All(c => c.BtmsDecisionCode.HasValue() && c.BtmsDecisionCode.StartsWith('X')),
            AnyMatch = alvsChecks.Any(c => !(c.BtmsDecisionCode.HasValue() && c.BtmsDecisionCode.StartsWith('X'))),
            AllNoMatch = alvsChecks.All(c => c.BtmsDecisionCode.HasValue() && c.BtmsDecisionCode.StartsWith('X')),
            AnyNoMatch = alvsChecks.Any(c => c.BtmsDecisionCode.HasValue() && c.BtmsDecisionCode.StartsWith('X')),
            AllRefuse = alvsChecks.All(c => c.BtmsDecisionCode.HasValue() && c.BtmsDecisionCode.StartsWith('N')),
            AnyRefuse = alvsChecks.Any(c => c.BtmsDecisionCode.HasValue() && c.BtmsDecisionCode.StartsWith('N')),
            AllRelease = alvsChecks.All(c => c.BtmsDecisionCode.HasValue() && c.BtmsDecisionCode.StartsWith('C')),
            AnyRelease = alvsChecks.Any(c => c.BtmsDecisionCode.HasValue() && c.BtmsDecisionCode.StartsWith('C')),
            AllHold = alvsChecks.All(c => c.BtmsDecisionCode.HasValue() && c.BtmsDecisionCode.StartsWith('H')),
            AnyHold = alvsChecks.Any(c => c.BtmsDecisionCode.HasValue() && c.BtmsDecisionCode.StartsWith('H'))
        };
    }
    
    private void CompareDecisions(AlvsDecision alvsDecision, CdsDecision btmsDecision)
    {
        GuardNullMovement();

        if (!alvsDecision.Context.DecisionComparison.HasValue())
        {
            throw new InvalidDataException("Should only be comparing when it's been paired");
        }
        
        var btmsCheckDictionary = btmsDecision.GetCheckDictionary();
        var alvsChecks = alvsDecision.GetItemChecks(btmsCheckDictionary);
        
        alvsDecision.Context.DecisionComparison.Checks = alvsChecks;
        alvsDecision.Context.AlvsCheckStatus = GetAlvsCheckStatus(alvsChecks);
        alvsDecision.Context.BtmsCheckStatus = GeBtmsCheckStatus(alvsChecks);

        var decisionStatus =
            decisionStatusFinder.GetDecisionStatus(_movement, alvsDecision);

        if (decisionStatus == DecisionStatusEnum.BtmsMadeSameDecisionAsAlvs)
        {
            alvsDecision.Context.DecisionComparison.DecisionMatched = true;
        }
        // var checksMatch = alvsChecks.All(c => c.AlvsDecisionCode == c.BtmsDecisionCode);

        // if (checksMatch)
        // {
        //     alvsDecision.Context.DecisionComparison.DecisionMatched = true;
        //     decisionStatus = DecisionStatusEnum.BtmsMadeSameDecisionAsAlvs;
        // }
        // else
        // {
        //     decisionStatus = decisionStatusFinder.GetDecisionStatus(_movement, alvsDecision);
        // }

        // else if (checkTypesMatch)
        // {
        //     decisionStatus = DecisionStatusEnum.BtmMadeSameDecisionTypeAsAlvs;
        // }
        // else if (_movement.Relationships.Notifications.Data.Count == 0)
        // {
        //     decisionStatus = DecisionStatusEnum.NoImportNotificationsLinked;
        // }
        // else if (_movement.AlvsDecisionStatus.Decisions.Count == 0)
        // {
        //     decisionStatus = DecisionStatusEnum.NoAlvsDecisions;
        // }
        // else if (_movement.BtmsStatus.ChedTypes.Contains(ImportNotificationTypeEnum.Chedpp))
        // {
        //     decisionStatus = DecisionStatusEnum.HasChedppChecks;
        // }
        
        alvsDecision.Context.DecisionComparison.DecisionStatus = decisionStatus;
    }

    public Movement Build()
    {
        GuardNullMovement();
        return _movement;
    }
}