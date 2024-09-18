namespace MudShowData.Models
{
    public class ProcedureDataModel
    {
        public string Description { get; set; }
        public string Code1 { get; set; }
        public string Code1Type { get; set; }
        public string Code2 { get; set; }
        public string Code2Type { get; set; }
        public string Code3 { get; set; }
        public string Code3Type { get; set; }
        public string Codei { get; set; }
        public string CodeiType { get; set; }
        public string Codeii { get; set; }
        public string CodeiiType { get; set; }
        public string Codeiii { get; set; }
        public string CodeiiiType { get; set; }
        public string BillingClass { get; set; }
        public string Setting { get; set; }
        public string DrugUnitOfMeasurement { get; set; }
        public string DrugTypeOfMeasurement { get; set; }
        public string StandardChargeGross { get; set; }
        public string StandardChargeDiscountedCash { get; set; }
        public string PayerName { get; set; }
        public string PlanName { get; set; }
        public string Modifiers { get; set; }
        public string StandardChargeNegotiatedDollar { get; set; }
        public string StandardChargeNegotiatedAlgorithm { get; set; }
        public string StandardChargeNegotiatedPercentage { get; set; }
        public string StandardChargeMin { get; set; }
        public string StandardChargeMax { get; set; }
        public string CountOfComparedRates { get; set; }
        public string StandardChargeMethodology { get; set; }
        public string AdditionalGenericNotes { get; set; }
        public string Footnote { get; set; }

        public HospitalDataModel Hospital { get; set; }
    }
}
