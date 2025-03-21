using System.Globalization;
using Btms.Analytics.Extensions;
using Btms.Backend.Data;
using Btms.Common.Enum;
using Btms.Model.Cds;
using Btms.Model.Ipaffs;
using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Btms.Analytics.Export;

public record MrnExportResult
{
    public required string Mrn { get; set; }
    public required string? FrontendLink { get; set; }
    public required string? HistoryLink { get; set; }
    public required string BusinessDecisionStatus { get; set; }

    public required string CreatedDate { get; set; }
    public required string LastUpdatedDate { get; set; }
    public required int ItemCount { get; set; }
    public required int CheckCount { get; set; }

    public required int ChecksMatching { get; set; }

    public required int ChecksNotMatching { get; set; }

    public required int AlvsDecisionCount { get; set; }

    public required int BtmsDecisionCount { get; set; }

    public required string? DecisionStatus { get; set; }
    public required string? Segment { get; set; }

    public required string? DecisionCategory { get; set; }

}

public record CheckExportResult : MrnExportResult
{
    public required string CheckCode { get; set; }
    public required string AlvsDecisionCode { get; set; }
    public required string BtmsDecisionCode { get; set; }
}

public class MovementExportService(IMongoDbContext context, ILogger<MovementExportService> logger)
{
    private readonly BusinessDecisionStatusEnum[] _exclude = [
        BusinessDecisionStatusEnum.MatchComplete,
        BusinessDecisionStatusEnum.MatchGroup
    ];

    public Task<IEnumerable<MrnExportResult>> GetMrnExport(DateTime from, DateTime to, bool finalisedOnly,
        ImportNotificationTypeEnum[]? chedTypes = null,
        string? country = null)
    {
        logger.LogInformation("GetMrnExport");

        var data = context
            .Movements
            .WhereFilteredByCreatedDateAndParams(from, to, finalisedOnly, chedTypes, country)
            .Where(m => !_exclude.Contains(m.Status.BusinessDecisionStatus))
            .Execute(logger)
            .Select(m => new MrnExportResult()
            {
                Mrn = m.Id!,
                BusinessDecisionStatus = m.Status.BusinessDecisionStatus.GetValue(),
                DecisionCategory = m.Status.NonComparableDecisionReason?.GetValue(),
                CreatedDate = m.CreatedSource!.Value.ToString("dd/MM/yyyy", CultureInfo.CurrentCulture),
                LastUpdatedDate = m.UpdatedSource!.Value.ToString("dd/MM/yyyy", CultureInfo.CurrentCulture),
                ItemCount = m.Items.Count,
                CheckCount = m.Items.Sum(i => i.Checks?.Length ?? 0),
                ChecksMatching = m.AlvsDecisionStatus.Context.DecisionComparison?.Checks
                    .Count(c => c.BtmsDecisionCode == c.AlvsDecisionCode) ?? 0,
                ChecksNotMatching = m.AlvsDecisionStatus.Context.DecisionComparison?.Checks
                    .Count(c => c.BtmsDecisionCode != c.AlvsDecisionCode) ?? 0,
                AlvsDecisionCount = m.AlvsDecisionStatus.Decisions.Count,
                BtmsDecisionCount = m.Decisions.Count,
                DecisionStatus = m.AlvsDecisionStatus.Context.DecisionComparison?.DecisionStatus.GetValue(),
                Segment = m.Status.Segment?.GetValue(),
                FrontendLink = $"https://btms-portal-frontend.test.cdp-int.defra.cloud/search-result?searchTerm={m.Id!}",
                HistoryLink = $"https://btms-frontend.test.cdp-int.defra.cloud/admin/view-history?mrn={m.Id!}"
            });

        return Task.FromResult(data);
    }

    public Task<IEnumerable<CheckExportResult>> GetCheckExport(DateTime from, DateTime to, bool finalisedOnly,
        ImportNotificationTypeEnum[]? chedTypes = null,
        string? country = null)
    {
        logger.LogInformation("GetCheckExport");
        return Task.FromResult((IEnumerable<CheckExportResult>)new List<CheckExportResult>());
    }

    public byte[] WriteAsCsv<T>(IEnumerable<T> data)
    {
        var config = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            NewLine = Environment.NewLine,
        };

        using var memoryStream = new MemoryStream();
        using var writer = new StreamWriter(memoryStream, leaveOpen: true);
        using var csv = new CsvWriter(writer, config, true);

        csv.WriteRecords(data);
        writer.Flush();
        memoryStream.Position = 0;

        return memoryStream.ToArray();
    }
}