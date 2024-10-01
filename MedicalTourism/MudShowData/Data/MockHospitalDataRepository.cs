using MudShowData.Models;

namespace MudShowData.Data
{
    public class MockHospitalDataRepository : IHospitalDataRepository
    {
        public MockHospitalDataRepository() { }

        public Task<List<AnthemCompanyModel>> GetAnthemCompanies()
        {
            List<string> plans = new List<string> { "Anthem Blue Cross", "Anthem Blue Shield", "Anthem Blue Cross Blue Shield",
                "Anthem Blue Cross of California",  "Anthem Blue Cross of Maryland", "Anthem Blue Cross of Virginia"};
            List<AnthemCompanyModel> mockData = new List<AnthemCompanyModel>() {
                new AnthemCompanyModel { CompanyName = "Michael and Son", EIN = "1", Plans = plans },
                new AnthemCompanyModel { CompanyName = "Bob and Sons", EIN = "2", Plans = plans },
                new AnthemCompanyModel { CompanyName = "Zodiak LLC", EIN = "3", Plans = plans },
                new AnthemCompanyModel { CompanyName = "Microsoft", EIN = "4", Plans = plans },
                new AnthemCompanyModel { CompanyName = "Macrosoft", EIN = "5" , Plans = plans},
                new AnthemCompanyModel { CompanyName = "Microstrategy", EIN = "6" , Plans = plans},
                new AnthemCompanyModel { CompanyName = "Microplastic", EIN = "7" , Plans = plans},
            };

            return Task.FromResult(mockData);
        }

        public Task<IEnumerable<ProcedureDataModel>> GetAnthemPlanDetails(string plan)
        {
            return GetHospitalDataAsync(new SearchProcedureModel { ProcedureName = plan });
        }

        public Task<IEnumerable<ProcedureDataModel>> GetHospitalDataAsync(SearchProcedureModel searchOptions)
        {
            List<ProcedureDataModel> mockData = new List<ProcedureDataModel>
            {
                new ProcedureDataModel
                {
                    Description = "Procedure 1",
                    Code1 = "001",
                    Code1Type = "Type1",
                    Code2 = "A01",
                    Code2Type = "TypeA",
                    Code3 = "X01",
                    Code3Type = "TypeX",
                    Codei = "I01",
                    CodeiType = "TypeI",
                    Codeii = "II01",
                    CodeiiType = "TypeII",
                    Codeiii = "III01",
                    CodeiiiType = "TypeIII",
                    BillingClass = "Class1",
                    Setting = "Setting1",
                    DrugUnitOfMeasurement = "mg",
                    DrugTypeOfMeasurement = "TypeM",
                    StandardChargeGross = "1000",
                    StandardChargeDiscountedCash = "900",
                    PayerName = "Payer1",
                    PlanName = "Plan1",
                    Modifiers = "Mod1",
                    StandardChargeNegotiatedDollar = "800",
                    StandardChargeNegotiatedAlgorithm = "Alg1",
                    StandardChargeNegotiatedPercentage = "80%",
                    StandardChargeMin = "700",
                    StandardChargeMax = "1100",
                    CountOfComparedRates = "5",
                    StandardChargeMethodology = "Method1",
                    AdditionalGenericNotes = "Notes1",
                    Footnote = "Footnote1",
                    Hospital = new HospitalDataModel { Name = "Hospital A", Location = "Location A", Address = "Address A" }
                },
                new ProcedureDataModel
                {
                    Description = "Procedure 2",
                    Code1 = "002",
                    Code1Type = "Type2",
                    Code2 = "B02",
                    Code2Type = "TypeB",
                    Code3 = "Y02",
                    Code3Type = "TypeY",
                    Codei = "I02",
                    CodeiType = "TypeI",
                    Codeii = "II02",
                    CodeiiType = "TypeII",
                    Codeiii = "III02",
                    CodeiiiType = "TypeIII",
                    BillingClass = "Class2",
                    Setting = "Setting2",
                    DrugUnitOfMeasurement = "ml",
                    DrugTypeOfMeasurement = "TypeL",
                    StandardChargeGross = "2000",
                    StandardChargeDiscountedCash = "1800",
                    PayerName = "Payer2",
                    PlanName = "Plan2",
                    Modifiers = "Mod2",
                    StandardChargeNegotiatedDollar = "1600",
                    StandardChargeNegotiatedAlgorithm = "Alg2",
                    StandardChargeNegotiatedPercentage = "75%",
                    StandardChargeMin = "1500",
                    StandardChargeMax = "2200",
                    CountOfComparedRates = "10",
                    StandardChargeMethodology = "Method2",
                    AdditionalGenericNotes = "Notes2",
                    Footnote = "Footnote2",
                    Hospital = new HospitalDataModel { Name = "Hospital B", Location = "Location B" }
                },
                new ProcedureDataModel
                {
                    Description = "Procedure 3",
                    Code1 = "003",
                    Code1Type = "Type3",
                    Code2 = "C03",
                    Code2Type = "TypeC",
                    Code3 = "Z03",
                    Code3Type = "TypeZ",
                    Codei = "I03",
                    CodeiType = "TypeI",
                    Codeii = "II03",
                    CodeiiType = "TypeII",
                    Codeiii = "III03",
                    CodeiiiType = "TypeIII",
                    BillingClass = "Class3",
                    Setting = "Setting3",
                    DrugUnitOfMeasurement = "g",
                    DrugTypeOfMeasurement = "TypeG",
                    StandardChargeGross = "3000",
                    StandardChargeDiscountedCash = "2700",
                    PayerName = "Payer3",
                    PlanName = "Plan3",
                    Modifiers = "Mod3",
                    StandardChargeNegotiatedDollar = "2400",
                    StandardChargeNegotiatedAlgorithm = "Alg3",
                    StandardChargeNegotiatedPercentage = "70%",
                    StandardChargeMin = "2100",
                    StandardChargeMax = "3300",
                    CountOfComparedRates = "15",
                    StandardChargeMethodology = "Method3",
                    AdditionalGenericNotes = "Notes3",
                    Footnote = "Footnote3",
                    Hospital = new HospitalDataModel { Name = "Hospital C", Location = "Location C" }
                }
            };

            return Task.FromResult<IEnumerable<ProcedureDataModel>>(mockData);
        }

    }
}

