namespace Cdms.Types.Ipaffs.Mapping;

public static class ImportNotificationWithTransformMapper
{
    public static Model.Ipaffs.ImportNotification MapWithTransform(this Types.Ipaffs.ImportNotification from)
    {
        if (from is null)
        {
            return default!;
        }

        var notification = ImportNotificationMapper.Map(from);
        Map(from, notification);
        return notification;
    }

    private static string FromSnakeCase(this string input)
    {
        if (input == "netweight")
        {
            return "netWeight";
        }

        var pascal = input.Split(new[] { "_" }, StringSplitOptions.RemoveEmptyEntries)
            .Select(s => char.ToUpperInvariant(s[0]) + s.Substring(1, s.Length - 1))
            .Aggregate(string.Empty, (s1, s2) => s1 + s2);
        return char.ToLower(pascal[0]) + pascal[1..];
    }

    private static IDictionary<string, object> FromSnakeCase(this IDictionary<string, object> input)
    {
        // var output = from pair in input
        //     select pair.Key;

        if (input == null)
        {
            return null;
        }

        return input.ToDictionary(mc => mc.Key.FromSnakeCase(),
            mc => mc.Value);

        // return output;
    }

    private static void Map(ImportNotification from, Model.Ipaffs.ImportNotification to)
    {
        var commodities = from.PartOne!.Commodities;

        if (commodities?.CommodityComplements?.Length == 1)
        {
            commodities!.CommodityComplements[0].AdditionalData =
                commodities!.ComplementParameterSets![0].KeyDataPairs!.FromSnakeCase();
            if (from.RiskAssessment != null)
            {
                commodities!.CommodityComplements[0].RiskAssesment = from.RiskAssessment.CommodityResults![0];
            }
        }
        else
        {
            var complementParameters = new Dictionary<int, ComplementParameterSet>();
            var complementRiskAssesments = new Dictionary<string, CommodityRiskResult>();
            var commodityChecks = new Dictionary<string, InspectionCheck[]>();

            foreach (var commoditiesCommodityComplement in commodities!.ComplementParameterSets!)
            {
                complementParameters[commoditiesCommodityComplement.ComplementId.Value!] =
                    commoditiesCommodityComplement;
            }

            if (from.RiskAssessment != null)
            {
                foreach (var commoditiesRa in from.RiskAssessment.CommodityResults!)
                {
                    complementRiskAssesments[commoditiesRa.UniqueId!] = commoditiesRa;
                }
            }

            if (from.PartTwo?.CommodityChecks != null)
            {
                foreach (var commodityCheck in from.PartTwo?.CommodityChecks)
                {
                    commodityChecks[commodityCheck.UniqueComplementId] = commodityCheck.Checks;
                }
            }

            if (commodities!.CommodityComplements is not null)
            {
                foreach (var commodity in commodities!.CommodityComplements)
                {
                    var parameters = complementParameters[commodity.ComplementId.Value!];
                    commodity.AdditionalData = parameters.KeyDataPairs.FromSnakeCase();

                    if (complementRiskAssesments.Any() &&
                        parameters.UniqueComplementId is not null &&
                        complementRiskAssesments.ContainsKey(parameters.UniqueComplementId))
                    {
                        commodity.RiskAssesment = complementRiskAssesments[parameters.UniqueComplementId!];
                    }

                    if (commodityChecks.Any() &&
                        parameters.UniqueComplementId is not null &&
                        commodityChecks.ContainsKey(parameters.UniqueComplementId))
                    {
                        commodity.Checks = commodityChecks[parameters.UniqueComplementId!];
                    }
                }
            }
        }

        to.CommoditiesSummary = CommoditiesMapper.Map(commodities);
        to.Commodities = commodities.CommodityComplements.Select(x => CommodityComplementMapper.Map(x)).ToArray();
    }
}