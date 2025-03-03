using System.Globalization;
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
    public required string BusinessDecisionStatus { get; set; }
    public required int ItemCount { get; set; }
}

public record CheckExportResult
{
    public required string Mrn { get; set; }
    public required string BusinessDecisionStatus { get; set; }
    public required string CheckCode { get; set; }
    public required string AlvsDecisionCode { get; set; }
    public required string BtmsDecisionCode { get; set; }
}

public class MovementExportService(ILogger<MovementExportService> logger)
{
    public Task<List<MrnExportResult>> GetMrnExport(DateTime from, DateTime to, bool finalisedOnly,
        ImportNotificationTypeEnum[]? chedTypes = null,
        string? country = null)
    {
        logger.LogInformation("GetMrnExport");
        var list = new List<MrnExportResult>();
        list.Add(new MrnExportResult()
        {
            Mrn = "AAA",
            ItemCount = 1,
            BusinessDecisionStatus = BusinessDecisionStatusEnum.AnythingElse.GetValue()
        });
        
        return Task.FromResult(list);
    }
    
    public Task<List<CheckExportResult>> GetCheckExport(DateTime from, DateTime to, bool finalisedOnly,
        ImportNotificationTypeEnum[]? chedTypes = null,
        string? country = null)
    {
        logger.LogInformation("GetCheckExport");
        return Task.FromResult(new List<CheckExportResult>());
    }
    
    public byte[] WriteAsCsv<T>(List<T> data)
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