using CsvHelper;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace ParseHospitalFile;

internal class ParseHospitalFile
{
    public string Filename { get; set; }
    public ParseHospitalFile(string filename)
    {
        Filename = filename;
    }
    public void Parse()
    {
        // first two lines contain hospital information - 1st line columns, 2nd data about hospital
        // third line contains the body column headers
        // the rest of the lines contain the body data
        using (var reader = new StreamReader(Filename))
        using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
        {
            csv.Read();
            csv.ReadHeader();
            var headers = csv.Context.HeaderRecord;
            Console.WriteLine(string.Join(", ", headers));
        }
    }

    public List<HospitalData> ParseHospitalData(string filePath)
{
    using (var reader = new StreamReader(filePath))
    using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
    {
        csv.Configuration.HasHeaderRecord = true;
        csv.Configuration.IgnoreBlankLines = true;
        csv.Configuration.Delimiter = ",";
        csv.Configuration.TrimOptions = CsvHelper.Configuration.TrimOptions.Trim;

        var records = csv.GetRecords<HospitalData>();
        return records.ToList();
    }
}

public List<HospitalBody> ParseHospitalBodies(string filePath)
{
    using (var reader = new StreamReader(filePath))
    using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
    {
        csv.Configuration.HasHeaderRecord = true;
        csv.Configuration.Delimiter = ",";
        csv.Configuration.IgnoreBlankLines = true;
        csv.Configuration.RegisterClassMap<HospitalBodyMap>();

        return csv.GetRecords<HospitalBody>().ToList();
    }
}

public sealed class HospitalBodyMap : ClassMap<HospitalBody>
{
    public HospitalBodyMap()
    {
        Map(m => m.Description).Name("description");
        Map(m => m.Code1).Name("code|1");
        Map(m => m.Code1Type).Name("code|1|type");
        Map(m => m.Code2).Name("code|2");
        Map(m => m.Code2Type).Name("code|2|type");
        Map(m => m.Code3).Name("code|3");
        Map(m => m.Code3Type).Name("code|3|type");
        Map(m => m.BillingClass).Name("billing_class");
        Map(m => m.Setting).Name("setting");
        Map(m => m.DrugUnitOfMeasurement).Name("drug_unit_of_measurement");
        Map(m => m.DrugTypeOfMeasurement).Name("drug_type_of_measurement");
        Map(m => m.StandardChargeGross).Name("standard_charge|gross");
        Map(m => m.StandardChargeDiscountedCash).Name("standard_charge|discounted_cash");
        Map(m => m.PayerName).Name("payer_name");
        Map(m => m.PlanName).Name("plan_name");
        Map(m => m.Modifiers).Name("modifiers");
        Map(m => m.StandardChargeNegotiatedDollar).Name("standard_charge|negotiated_dollar");
        Map(m => m.StandardChargeNegotiatedAlgorithm).Name("standard_charge|negotiated_algorithm");
        Map(m => m.StandardChargeNegotiatedPercentage).Name("standard_charge|negotiated_percentage");
        Map(m => m.StandardChargeMin).Name("standard_charge|min");
        Map(m => m.StandardChargeMax).Name("standard_charge|max");
        Map(m => m.CountOfComparedRates).Name("count_of_compared_rates");
        Map(m => m.StandardChargeMethodology).Name("standard_charge|methodology");
        Map(m => m.AdditionalGenericNotes).Name("additional_generic_notes");
        Map(m => m.Footnote).Name("footnote");
    }
}
}

public void InsertHospitalData(HospitalData hospitalData)
    {
        using (MySqlConnection conn = new MySqlConnection(connectionString))
        {
            conn.Open();
            string query = @"
                INSERT INTO HospitalData (
                    Description, Code1, Code1Type, Code2, Code2Type, Code3, Code3Type, BillingClass, Setting, 
                    DrugUnitOfMeasurement, DrugTypeOfMeasurement, StandardChargeGross, StandardChargeDiscountedCash, 
                    PayerName, PlanName, Modifiers, StandardChargeNegotiatedDollar, StandardChargeNegotiatedAlgorithm, 
                    StandardChargeNegotiatedPercentage, StandardChargeMin, StandardChargeMax, CountOfComparedRates, 
                    StandardChargeMethodology, AdditionalGenericNotes, Footnote
                ) VALUES (
                    @Description, @Code1, @Code1Type, @Code2, @Code2Type, @Code3, @Code3Type, @BillingClass, @Setting, 
                    @DrugUnitOfMeasurement, @DrugTypeOfMeasurement, @StandardChargeGross, @StandardChargeDiscountedCash, 
                    @PayerName, @PlanName, @Modifiers, @StandardChargeNegotiatedDollar, @StandardChargeNegotiatedAlgorithm, 
                    @StandardChargeNegotiatedPercentage, @StandardChargeMin, @StandardChargeMax, @CountOfComparedRates, 
                    @StandardChargeMethodology, @AdditionalGenericNotes, @Footnote
                )";

            using (MySqlCommand cmd = new MySqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@Description", hospitalData.Description);
                cmd.Parameters.AddWithValue("@Code1", hospitalData.Code1);
                cmd.Parameters.AddWithValue("@Code1Type", hospitalData.Code1Type);
                cmd.Parameters.AddWithValue("@Code2", hospitalData.Code2);
                cmd.Parameters.AddWithValue("@Code2Type", hospitalData.Code2Type);
                cmd.Parameters.AddWithValue("@Code3", hospitalData.Code3);
                cmd.Parameters.AddWithValue("@Code3Type", hospitalData.Code3Type);
                cmd.Parameters.AddWithValue("@BillingClass", hospitalData.BillingClass);
                cmd.Parameters.AddWithValue("@Setting", hospitalData.Setting);
                cmd.Parameters.AddWithValue("@DrugUnitOfMeasurement", hospitalData.DrugUnitOfMeasurement);
                cmd.Parameters.AddWithValue("@DrugTypeOfMeasurement", hospitalData.DrugTypeOfMeasurement);
                cmd.Parameters.AddWithValue("@StandardChargeGross", hospitalData.StandardChargeGross);
                cmd.Parameters.AddWithValue("@StandardChargeDiscountedCash", hospitalData.StandardChargeDiscountedCash);
                cmd.Parameters.AddWithValue("@PayerName", hospitalData.PayerName);
                cmd.Parameters.AddWithValue("@PlanName", hospitalData.PlanName);
                cmd.Parameters.AddWithValue("@Modifiers", hospitalData.Modifiers);
                cmd.Parameters.AddWithValue("@StandardChargeNegotiatedDollar", hospitalData.StandardChargeNegotiatedDollar);
                cmd.Parameters.AddWithValue("@StandardChargeNegotiatedAlgorithm", hospitalData.StandardChargeNegotiatedAlgorithm);
                cmd.Parameters.AddWithValue("@StandardChargeNegotiatedPercentage", hospitalData.StandardChargeNegotiatedPercentage);
                cmd.Parameters.AddWithValue("@StandardChargeMin", hospitalData.StandardChargeMin);
                cmd.Parameters.AddWithValue("@StandardChargeMax", hospitalData.StandardChargeMax);
                cmd.Parameters.AddWithValue("@CountOfComparedRates", hospitalData.CountOfComparedRates);
                cmd.Parameters.AddWithValue("@StandardChargeMethodology", hospitalData.StandardChargeMethodology);
                cmd.Parameters.AddWithValue("@AdditionalGenericNotes", hospitalData.AdditionalGenericNotes);
                cmd.Parameters.AddWithValue("@Footnote", hospitalData.Footnote);

                cmd.ExecuteNonQuery();
            }
        }
    }
}

public void InsertHospitalBody(HospitalBody hospitalBody)
    {
        using (MySqlConnection conn = new MySqlConnection(connectionString))
        {
            conn.Open();
            string query = @"
                INSERT INTO HospitalBody (
                    Description, Code1, Code1Type, Code2, Code2Type, Code3, Code3Type, BillingClass, Setting, 
                    DrugUnitOfMeasurement, DrugTypeOfMeasurement, StandardChargeGross, StandardChargeDiscountedCash, 
                    PayerName, PlanName, Modifiers, StandardChargeNegotiatedDollar, StandardChargeNegotiatedAlgorithm, 
                    StandardChargeNegotiatedPercentage, StandardChargeMin, StandardChargeMax, CountOfComparedRates, 
                    StandardChargeMethodology, AdditionalGenericNotes, Footnote
                ) VALUES (
                    @Description, @Code1, @Code1Type, @Code2, @Code2Type, @Code3, @Code3Type, @BillingClass, @Setting, 
                    @DrugUnitOfMeasurement, @DrugTypeOfMeasurement, @StandardChargeGross, @StandardChargeDiscountedCash, 
                    @PayerName, @PlanName, @Modifiers, @StandardChargeNegotiatedDollar, @StandardChargeNegotiatedAlgorithm, 
                    @StandardChargeNegotiatedPercentage, @StandardChargeMin, @StandardChargeMax, @CountOfComparedRates, 
                    @StandardChargeMethodology, @AdditionalGenericNotes, @Footnote
                )";

            using (MySqlCommand cmd = new MySqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@Description", hospitalBody.Description);
                cmd.Parameters.AddWithValue("@Code1", hospitalBody.Code1);
                cmd.Parameters.AddWithValue("@Code1Type", hospitalBody.Code1Type);
                cmd.Parameters.AddWithValue("@Code2", hospitalBody.Code2);
                cmd.Parameters.AddWithValue("@Code2Type", hospitalBody.Code2Type);
                cmd.Parameters.AddWithValue("@Code3", hospitalBody.Code3);
                cmd.Parameters.AddWithValue("@Code3Type", hospitalBody.Code3Type);
                cmd.Parameters.AddWithValue("@BillingClass", hospitalBody.BillingClass);
                cmd.Parameters.AddWithValue("@Setting", hospitalBody.Setting);
                cmd.Parameters.AddWithValue("@DrugUnitOfMeasurement", hospitalBody.DrugUnitOfMeasurement);
                cmd.Parameters.AddWithValue("@DrugTypeOfMeasurement", hospitalBody.DrugTypeOfMeasurement);
                cmd.Parameters.AddWithValue("@StandardChargeGross", hospitalBody.StandardChargeGross);
                cmd.Parameters.AddWithValue("@StandardChargeDiscountedCash", hospitalBody.StandardChargeDiscountedCash);
                cmd.Parameters.AddWithValue("@PayerName", hospitalBody.PayerName);
                cmd.Parameters.AddWithValue("@PlanName", hospitalBody.PlanName);
                cmd.Parameters.AddWithValue("@Modifiers", hospitalBody.Modifiers);
                cmd.Parameters.AddWithValue("@StandardChargeNegotiatedDollar", hospitalBody.StandardChargeNegotiatedDollar);
                cmd.Parameters.AddWithValue("@StandardChargeNegotiatedAlgorithm", hospitalBody.StandardChargeNegotiatedAlgorithm);
                cmd.Parameters.AddWithValue("@StandardChargeNegotiatedPercentage", hospitalBody.StandardChargeNegotiatedPercentage);
                cmd.Parameters.AddWithValue("@StandardChargeMin", hospitalBody.StandardChargeMin);
                cmd.Parameters.AddWithValue("@StandardChargeMax", hospitalBody.StandardChargeMax);
                cmd.Parameters.AddWithValue("@CountOfComparedRates", hospitalBody.CountOfComparedRates);
                cmd.Parameters.AddWithValue("@StandardChargeMethodology", hospitalBody.StandardChargeMethodology);
                cmd.Parameters.AddWithValue("@AdditionalGenericNotes", hospitalBody.AdditionalGenericNotes);
                cmd.Parameters.AddWithValue("@Footnote", hospitalBody.Footnote);

                cmd.ExecuteNonQuery();
            }
        }
    }
}    

public class HospitalData
{
    public string Description { get; set; }
    public string Code1 { get; set; }
    public string Code1Type { get; set; }
    public string Code2 { get; set; }
    public string Code2Type { get; set; }
    public string Code3 { get; set; }
    public string Code3Type { get; set; }
    public string BillingClass { get; set; }
    public string Setting { get; set; }
    public string DrugUnitOfMeasurement { get; set; }
    public string DrugTypeOfMeasurement { get; set; }
    public decimal StandardChargeGross { get; set; }
    public decimal StandardChargeDiscountedCash { get; set; }
    public string PayerName { get; set; }
    public string PlanName { get; set; }
    public string Modifiers { get; set; }
    public decimal StandardChargeNegotiatedDollar { get; set; }
    public string StandardChargeNegotiatedAlgorithm { get; set; }
    public decimal StandardChargeNegotiatedPercentage { get; set; }
    public decimal StandardChargeMin { get; set; }
    public decimal StandardChargeMax { get; set; }
    public int CountOfComparedRates { get; set; }
    public string StandardChargeMethodology { get; set; }
    public string AdditionalGenericNotes { get; set; }
    public string Footnote { get; set; }
}

public class HospitalBody
{
    public string Description { get; set; }
    public string Code1 { get; set; }
    public string Code1Type { get; set; }
    public string Code2 { get; set; }
    public string Code2Type { get; set; }
    public string Code3 { get; set; }
    public string Code3Type { get; set; }
    public string BillingClass { get; set; }
    public string Setting { get; set; }
    public string DrugUnitOfMeasurement { get; set; }
    public string DrugTypeOfMeasurement { get; set; }
    public decimal StandardChargeGross { get; set; }
    public decimal StandardChargeDiscountedCash { get; set; }
    public string PayerName { get; set; }
    public string PlanName { get; set; }
    public string Modifiers { get; set; }
    public decimal StandardChargeNegotiatedDollar { get; set; }
    public string StandardChargeNegotiatedAlgorithm { get; set; }
    public decimal StandardChargeNegotiatedPercentage { get; set; }
    public decimal StandardChargeMin { get; set; }
    public decimal StandardChargeMax { get; set; }
    public int CountOfComparedRates { get; set; }
    public string StandardChargeMethodology { get; set; }
    public string AdditionalGenericNotes { get; set; }
    public string Footnote { get; set; }
}
