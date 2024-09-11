using CsvHelper;
using CsvHelper.Configuration;
using MySql.Data.MySqlClient;
using System.Globalization;
using System.Text.RegularExpressions;

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
        Console.WriteLine($"Parsing {Filename}");
        // first two lines contain hospital information - 1st line columns, 2nd data about hospital
        // third line contains the body column headers
        // the rest of the lines contain the body data

        CsvConfiguration config = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            MissingFieldFound = null
        };

        //config.PrepareHeaderForMatch = static header => Regex.Replace(header, @"\s", string.Empty);
        config.PrepareHeaderForMatch = static args => Regex.Replace(args.Header, @"\s", string.Empty);

        using (StreamReader reader = new StreamReader(Filename))
        using (CsvReader csv = new CsvReader(reader, config))
        {
            csv.Read();
            csv.ReadHeader();
            csv.Context.RegisterClassMap<HospitalDataMap>();
            string[]? headers = csv.HeaderRecord;
            Console.WriteLine(string.Join(", ", headers));

            csv.Read();
            HospitalData dd = csv.GetRecord<HospitalData>();

            csv.Read();
            csv.ReadHeader();
            csv.Context.RegisterClassMap<HospitalBodyMap>();
            headers = csv.HeaderRecord;
            Console.WriteLine(string.Join(", ", headers));

            int count = 0;
            while (csv.Read())
            {
                HospitalBody bb = csv.GetRecord<HospitalBody>();
                Console.WriteLine($"Code1: {bb.Code1}, Gross: {bb.StandardChargeGross}");
                count++;
            }
            Console.WriteLine($"Parsed {count} body records");
        }
    }

    public void InsertHospitalData(HospitalData hospitalData, string connectionString)
    {
        using (MySqlConnection conn = new MySqlConnection(connectionString))
        {
            conn.Open();
            string query = @"
                INSERT INTO HospitalData (
                    Name, LastUpdated, AsOfDate, Version, Location, Address, License, FinancialAid, Belief
                ) VALUES (
                    @Name, @LastUpdated, @AsOfDate, @Version, @Location, @Address, @License, @FinancialAid, @Belief
                )";

            using (MySqlCommand cmd = new MySqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@Name", hospitalData.hospital_name);
                cmd.Parameters.AddWithValue("@LastUpdated", hospitalData.last_updated_on);
                cmd.Parameters.AddWithValue("@AsOfDate", hospitalData.as_of_date);
                cmd.Parameters.AddWithValue("@Version", hospitalData.version);
                cmd.Parameters.AddWithValue("@Location", hospitalData.hospital_location);
                cmd.Parameters.AddWithValue("@Address", hospitalData.hospital_address);
                cmd.Parameters.AddWithValue("@License", hospitalData.license_number);
                cmd.Parameters.AddWithValue("@FinancialAid", hospitalData.financial_aid_policy);
                cmd.Parameters.AddWithValue("@Belief", hospitalData.belief);

                cmd.ExecuteNonQuery();
            }
        }
    }


    public void InsertHospitalBody(HospitalBody hospitalBody, string connectionString)
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


public class HospitalData
{
    public string hospital_name { get; set; }
    public string last_updated_on { get; set; }
    public string as_of_date { get; set; }
    public string version { get; set; }
    public string hospital_location { get; set; }
    public string hospital_address { get; set; }
    public string license_number { get; set; }
    public string financial_aid_policy { get; set; }
    public string belief { get; set; }
}

public class HospitalDataMap : ClassMap<HospitalData>
{
    public HospitalDataMap()
    {
        Map(m => m.hospital_name).Index(0);
        Map(m => m.last_updated_on).Index(1);
        Map(m => m.as_of_date).Index(2);
        Map(m => m.version).Index(3);
        Map(m => m.hospital_location).Index(4);
        Map(m => m.hospital_address).Index(5);
        Map(m => m.license_number).Index(6);
        Map(m => m.financial_aid_policy).Index(7);
        Map(m => m.belief).Index(8);

    }
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
}
