using Btms.Model.Alvs;
using Btms.Model.Ipaffs;

namespace Btms.Business.Services.Decisions;

public record DecisionResult
{
    private readonly List<DocumentDecisionResult> _results = [];
    public DocumentDecisionResult AddDecision(string movementId, int itemNumber, string documentReference, DecisionCode decisionCode)
    {
        var item = new DocumentDecisionResult(movementId, itemNumber, documentReference, decisionCode);
        _results.Add(item);
        return item;
    }

    public IReadOnlyList<DocumentDecisionResult> Decisions => _results.AsReadOnly();
}

////public record MovementDecisionResult(string EntryReference, int EntryVersion)
////{
////    private readonly List<ItemDecisionResult> _results = [];
////    public ItemDecisionResult AddDecision(Items item)
////    {
////        var i = new ItemDecisionResult(item);
////        _results.Add(i);
////        return i;
////    }

////    public IReadOnlyList<ItemDecisionResult> ItemDecisions => _results.AsReadOnly();

   
////}

////public record ItemDecisionResult(Items Item)
////{
////    private readonly List<DocumentDecisionResult> _results = [];
////    public DocumentDecisionResult AddDecision(Document document, DecisionCode decisionCode)
////    {
////        var item = new DocumentDecisionResult(document, decisionCode);
////        _results.Add(item);
////        return item;
////    }

////    public IReadOnlyList<DocumentDecisionResult> DocumentDecisions => _results.AsReadOnly();

////    public DecisionCode GetDecisionCode()
////    {
////        return _results.Max(x => x.DecisionCode);
////    }

////    public string[] GetDecisionReasons()
////    {
////        switch (GetDecisionCode())
////        {
////            case DecisionCode.X00:
////                var chedType = MapToChedType(DocumentDecisions[0].Document.DocumentCode!);
////                var chedNumbers = string.Join(',', DocumentDecisions.Select(x => x.Document.DocumentReference));
////                return
////                [
////                    $"A Customs Declaration has been submitted however no matching {chedType}(s) have been submitted to Port Health (for {chedType} number(s) {chedNumbers}). Please correct the {chedType} number(s) entered on your customs declaration."
////                ];
////        }

////        return [];
////    }

////    private static string MapToChedType(string documentCode)
////    {
////        return documentCode switch
////        {
////            "N002" or "N851" or "9115" => ImportNotificationTypeEnum.Chedpp.ToString(),
////            "N852" or "C678" => ImportNotificationTypeEnum.Ced.ToString(),
////            "C640" => ImportNotificationTypeEnum.Cveda.ToString(),
////            "C641" or "C673" or "N853" => ImportNotificationTypeEnum.Cvedp.ToString(),
////            _ => throw new ArgumentOutOfRangeException(nameof(documentCode), documentCode, null)
////        };


////    }
////}

public record DocumentDecisionResult(string MovementId, int ItemNumber, string DocumentReference, DecisionCode DecisionCode);

////public record NoMatchDecision(string DocumentReference, int ItemNumber, string[] CheckCodes, ImportNotificationTypeEnum ChedType)
////{
////    public string Reason =>
////        $"A Customs Declaration has been submitted however no matching {ChedType.ToString()}(s) have been submitted to Port Health (for {ChedType.ToString()} number(s) {DocumentReference}). Please correct the {ChedType.ToString()} number(s) entered on your customs declaration.";
////}

////public record DecisionClearanceRequest(string Id, int Version, List<DecisionItem> Items);

////public record DecisionItem(int ItemNumber, List<DecisionCheck> Checks);

////public record DecisionCheck(string CheckCode, DecisionCode DecisionCode, DateTime? DecisionValidUntil, string? Reason);

//list of Crs
//each CR has a list of items

public record DecisionFinderResult(DecisionCode DecisionCode);
