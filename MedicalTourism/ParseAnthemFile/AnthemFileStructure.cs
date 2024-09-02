using Newtonsoft.Json;

namespace ParseAnthemFile;

public class ReportingData
{
    [JsonProperty("reporting_entity_name")]
    public string ReportingEntityName { get; set; }
    [JsonProperty("reporting_entity_type")]
    public string ReportingEntityType { get; set; }
    [JsonProperty("reporting_structure")]
    public List<ReportingStructure> ReportingStructure { get; set; }
}

public class ReportingStructure
{
    [JsonProperty("reporting_plans")]
    public List<ReportingPlan> ReportingPlans { get; set; }

    [JsonProperty("in_network_files")]
    public List<InNetworkFile> InNetworkFiles { get; set; }

    [JsonProperty("allowed_amount_file")]
    public AllowedAmountFile AllowedAmountFile { get; set; }
}

public class ReportingPlan
{
    [JsonProperty("plan_name")]
    public string PlanName { get; set; }

    [JsonProperty("plan_id_type")]
    public string PlanIdType { get; set; }

    [JsonProperty("plan_id")]
    public string PlanId { get; set; }

    [JsonProperty("plan_market_type")]
    public string PlanMarketType { get; set; }
}

public class InNetworkFile
{
    [JsonProperty("description")]
    public string Description { get; set; }

    [JsonProperty("location")]
    public string Location { get; set; }
}

public class AllowedAmountFile
{
    [JsonProperty("description")]
    public string Description { get; set; }

    [JsonProperty("location")]
    public string Location { get; set; }
}