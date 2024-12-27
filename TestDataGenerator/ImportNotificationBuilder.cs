using Btms.Common.Extensions;
using Btms.Types.Ipaffs;
using TestDataGenerator.Helpers;

namespace TestDataGenerator;

public class ImportNotificationBuilder : ImportNotificationBuilder<ImportNotification>
{
    private ImportNotificationBuilder()
    {
    }

    private ImportNotificationBuilder(string file) : base(file)
    {
    }

    public static ImportNotificationBuilder<ImportNotification> FromFile(string file)
    {
        return new ImportNotificationBuilder(file)
            .WithClean();
    }

    public static ImportNotificationBuilder<ImportNotification> Default()
    {
        return new ImportNotificationBuilder()
            .WithClean();
    }
}

public class ImportNotificationBuilder<T> : BuilderBase<T, ImportNotificationBuilder<T>>
    where T : ImportNotification, new()
{
    protected ImportNotificationBuilder()
    {
    }

    protected ImportNotificationBuilder(string file) : base(file)
    {
    }
    
    /// <summary>
    ///     Allows any customisations needed, such as removing problems with serialisation, e.g Do(n =>
    ///     Array.ForEach(n.PartOne!.Commodities!.ComplementParameterSets!, x => x.KeyDataPairs = null));
    /// </summary>
    protected ImportNotificationBuilder<T> WithClean()
    {
        return this;
    }

    public ImportNotificationBuilder<T> WithRandomCommodities(int min, int max)
    {
        var commodityCount = CreateRandomInt(min, max);

        return Do(n =>
        {
            var commodities = Enumerable.Range(0, commodityCount)
                .Select(_ => n.PartOne!.Commodities!.CommodityComplements![0]
                ).ToArray();

            n.PartOne!.Commodities!.CommodityComplements = commodities;
        });
    }
    
    public ImportNotificationBuilder<T> WithReferenceNumber(ImportNotificationTypeEnum chedType, int scenario,
         DateTime created, int item)
    {
        return Do(x =>
            x.ReferenceNumber = DataHelpers.GenerateReferenceNumber(chedType, scenario, created, item));
    }

    public ImportNotificationBuilder<T> WithCreationDate(DateTime entryDate, bool randomTime = true)
    {
        var entry = randomTime ?
            // We don't want documents created in the future!
            entryDate.RandomTime(entryDate.Date == DateTime.Today ? DateTime.Now.AddHours(-2).Hour : 23)
            : entryDate;
        
        return Do(x => x.LastUpdated = entry);
    }

    public ImportNotificationBuilder<T> WithRandomArrivalDateTime(int maxDays, int maxHours=6)
    {
        var dayOffset = CreateRandomInt(maxDays * -1, maxDays);
        var hourOffset = CreateRandomInt(maxHours * -1, maxHours);
        
        return Do(x =>
        {
            var dt = DateTime.Today.AddDays(dayOffset).AddHours(hourOffset);
            dt = dt > x.LastUpdated ? dt : x.LastUpdated ?? dt;
            x.PartOne!.ArrivalDate = dt.ToDate();
            x.PartOne!.ArrivalTime = dt.ToTime();
        });
    }
    
    

    public ImportNotificationBuilder<T> WithSimpleCommodity(string commodityCode, string description, int netWeight, Guid? uniqueComplementId = null)
    {
        return Do(n =>
        {
            n.PartOne!.Commodities = new Commodities()
            {
                CommodityComplements = [
                    new CommodityComplement()
                    {
                        ComplementId = 1,
                        SpeciesId = "000",
                        SpeciesClass = "XXXX",
                        SpeciesName = "XXXX",
                        CommodityDescription = description,
                        ComplementName = "XXXX",
                        SpeciesNomination = "XXXX",
                    }
                ],
                ComplementParameterSets = [
                    new ComplementParameterSet()
                    {
                        UniqueComplementId = (uniqueComplementId ?? Guid.NewGuid()).ToString(),
                        ComplementId = 1,
                        SpeciesId = "000",
                        KeyDataPairs = new Dictionary<string, object>()
                        {
                            { "netweight", netWeight }
                        }
                    }
                ],
                TotalNetWeight = netWeight,
                TotalGrossWeight = netWeight,
                
            };
            
            // n.PartOne!.Commodities!.TotalNetWeight = netWeight;
            // n.PartOne!.Commodities!.TotalGrossWeight = netWeight;
            // n.PartOne!.Commodities!.CommodityComplements![0].SpeciesId = "000";
            // n.PartOne!.Commodities!.CommodityComplements![0].SpeciesClass = "XXXX";
            // n.PartOne!.Commodities!.CommodityComplements![0].SpeciesName = "XXXX";
            // n.PartOne!.Commodities!.CommodityComplements![0].CommodityDescription = description;
            // n.PartOne!.Commodities!.CommodityComplements![0].ComplementName = "XXXX";
            // n.PartOne!.Commodities!.CommodityComplements![0].SpeciesNomination = "XXXX";
            // n.PartOne!.Commodities!.ComplementParameterSets![0].SpeciesId = "000";
            // n.PartOne!.Commodities!.ComplementParameterSets![0].KeyDataPairs!["netweight"] = netWeight;
        });
    }
    
    public ImportNotificationBuilder<T> WithNoCommodities()
    {
        return Do(n =>
        {
            n.RiskAssessment = null;
            n.PartOne!.Commodities = null;
        });
    }
    
    public ImportNotificationBuilder<T> WithRiskAssesment(Guid uniqueComplementId, CommodityRiskResultRiskDecisionEnum riskDecision)
    {
        return Do(n =>
        {
            n.RiskAssessment = new RiskAssessmentResult()
            {
                CommodityResults = new []
                {
                    new CommodityRiskResult()
                    {
                        RiskDecision = riskDecision,
                        UniqueId = uniqueComplementId.ToString()
                    }
                }
            };

        });
    }
    
    public ImportNotificationBuilder<T> WithInspectionStatus(string inspectionRequired = "Required")
    {
        return Do(x =>
        {
            x.PartTwo!.InspectionRequired = inspectionRequired;
        });
    }

    public ImportNotificationBuilder<T> WithVersionNumber(int version = 1)
    {
        return Do(x =>
        {
            x.Version = version;
        });
    }
    protected override ImportNotificationBuilder<T> Validate()
    {
        return Do(n =>
        {
            n.ReferenceNumber.AssertHasValue("Import Notification ReferenceNumber missing");
            n.PartOne!.ArrivalDate.AssertHasValue("Import Notification PartOne ArrivalDate missing");
            n.PartOne!.ArrivalTime.AssertHasValue("Import Notification PartOne ArrivalTime missing");
            n.Version.AssertHasValue("Import Notification Version missing");
            
            // NB - this may not be correct...
            if (n.ImportNotificationType != ImportNotificationTypeEnum.Cveda)
            {
                n.PartTwo!.InspectionRequired.AssertHasValue("Import Notification PartTwo InspectionRequired missing");    
            }
            
        });
    }
}