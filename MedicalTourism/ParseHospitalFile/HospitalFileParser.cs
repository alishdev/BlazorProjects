using CsvHelper;
using CsvHelper.Configuration;
using MySql.Data.MySqlClient;
using System.Configuration;
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
            string[]? headers = csv.HeaderRecord;
            //Console.WriteLine(string.Join(", ", headers));
            if (headers.Contains("as_of_date"))
                csv.Context.RegisterClassMap<HospitalDataMapTall>();
            else
                csv.Context.RegisterClassMap<HospitalDataMapWide>();
            csv.Read();

            string connectionString = ConfigurationManager.ConnectionStrings["myConnectionString"].ConnectionString;
            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                conn.Open();
                using (MySqlTransaction transaction = conn.BeginTransaction())
                {

                    HospitalData hospitalData = csv.GetRecord<HospitalData>();
                    int hospital = InsertHospitalData(hospitalData, conn, transaction);

                    csv.Read();
                    csv.ReadHeader();
                    csv.Context.RegisterClassMap<HospitalBodyMap>();
                    //headers = csv.HeaderRecord;
                    //Console.WriteLine(string.Join(", ", headers));

                    int count = 0;
                    while (csv.Read())
                    {
                        HospitalBody procedure = csv.GetRecord<HospitalBody>();
                        InsertHospitalBody(hospital, procedure, conn, transaction);
                        //Console.WriteLine($"Code1: {procedure.Code1}, Gross: {procedure.StandardChargeGross}");
                        count++;
                        if (count % 1000 == 0)
                        {
                            //break;
                            Console.WriteLine($"Parsed {count} body records");
                        }
                    }
                    Console.WriteLine($"Parsed {count} body records");
                    transaction.Commit();

                }
            }
        }
    }

    public int InsertHospitalData(HospitalData hospitalData, MySqlConnection conn, MySqlTransaction transaction)
    {

        string query = @"
                INSERT INTO HospitalData (
                    Name, LastUpdated, AsOfDate, Version, Location, Address, License, FinancialAid, Belief
                ) VALUES (
                    @Name, @LastUpdated, @AsOfDate, @Version, @Location, @Address, @License, @FinancialAid, @Belief
                )";

        using (MySqlCommand cmd = new MySqlCommand(query, conn, transaction))
        {
            cmd.Parameters.AddWithValue("@Name", hospitalData.hospital_name);
            cmd.Parameters.AddWithValue("@LastUpdated", ToDate(hospitalData.last_updated_on));
            cmd.Parameters.AddWithValue("@AsOfDate", ToDate(hospitalData.as_of_date));
            cmd.Parameters.AddWithValue("@Version", hospitalData.version);
            cmd.Parameters.AddWithValue("@Location", hospitalData.hospital_location);
            cmd.Parameters.AddWithValue("@Address", hospitalData.hospital_address);
            cmd.Parameters.AddWithValue("@License", hospitalData.license_number);
            cmd.Parameters.AddWithValue("@FinancialAid", ToString(hospitalData.financial_aid_policy));
            cmd.Parameters.AddWithValue("@Belief", ToBool(hospitalData.belief));
            cmd.ExecuteNonQuery();
            long id = cmd.LastInsertedId;
            return Convert.ToInt32(id);
        }

    }

    private object ToString(string par) => string.IsNullOrEmpty(par) ? DBNull.Value : par;

    private DateTime? ToDate(string par)
    {
        if (DateTime.TryParse(par, out DateTime date))
        {
            return date;
        }
        return null;
    }

    private decimal? ToDecimal(string par)
    {
        if (decimal.TryParse(par, out decimal dec))
        {
            return dec;
        }
        return null;
    }

    private bool ToBool(string par)
    {
        if (par != null && par.ToLower() == "true")
            return true;
        return false;
    }

    private int? ToInt(string par)
    {
        if (int.TryParse(par, out int i))
        {
            return i;
        }
        return null;
    }

    public void InsertHospitalBody(int hospitalDataId, HospitalBody hospitalBody, MySqlConnection conn, MySqlTransaction transaction)
    {

        string query = @"
                INSERT INTO HospitalBody (HospitalDataId,
                    Description, Code1, Code1Type, Code2, Code2Type, Code3, Code3Type, BillingClass, Setting, 
                    DrugUnitOfMeasurement, DrugTypeOfMeasurement, StandardChargeGross, StandardChargeDiscountedCash, 
                    PayerName, PlanName, Modifiers, StandardChargeNegotiatedDollar, StandardChargeNegotiatedAlgorithm, 
                    StandardChargeNegotiatedPercentage, StandardChargeMin, StandardChargeMax, CountOfComparedRates, 
                    StandardChargeMethodology, AdditionalGenericNotes, Footnote
                ) VALUES (@HospitalDataId,
                    @Description, @Code1, @Code1Type, @Code2, @Code2Type, @Code3, @Code3Type, @BillingClass, @Setting, 
                    @DrugUnitOfMeasurement, @DrugTypeOfMeasurement, @StandardChargeGross, @StandardChargeDiscountedCash, 
                    @PayerName, @PlanName, @Modifiers, @StandardChargeNegotiatedDollar, @StandardChargeNegotiatedAlgorithm, 
                    @StandardChargeNegotiatedPercentage, @StandardChargeMin, @StandardChargeMax, @CountOfComparedRates, 
                    @StandardChargeMethodology, @AdditionalGenericNotes, @Footnote
                )";

        using (MySqlCommand cmd = new MySqlCommand(query, conn, transaction))
        {
            cmd.Parameters.AddWithValue("@HospitalDataId", hospitalDataId);
            cmd.Parameters.AddWithValue("@Description", hospitalBody.Description);
            cmd.Parameters.AddWithValue("@Code1", ToString(hospitalBody.Code1 ?? hospitalBody.Codei));
            cmd.Parameters.AddWithValue("@Code1Type", ToString(hospitalBody.Code1Type ?? hospitalBody.CodeiType));
            cmd.Parameters.AddWithValue("@Code2", ToString(hospitalBody.Code2 ?? hospitalBody.Codeii));
            cmd.Parameters.AddWithValue("@Code2Type", ToString(hospitalBody.Code2Type ?? hospitalBody.CodeiiType));
            cmd.Parameters.AddWithValue("@Code3", ToString(hospitalBody.Code3 ?? hospitalBody.Codeiii));
            cmd.Parameters.AddWithValue("@Code3Type", ToString(hospitalBody.Code3Type ?? hospitalBody.CodeiiiType));
            cmd.Parameters.AddWithValue("@BillingClass", ToString(hospitalBody.BillingClass));
            cmd.Parameters.AddWithValue("@Setting", ToString(hospitalBody.Setting));
            cmd.Parameters.AddWithValue("@DrugUnitOfMeasurement", ToString(hospitalBody.DrugUnitOfMeasurement));
            cmd.Parameters.AddWithValue("@DrugTypeOfMeasurement", ToString(hospitalBody.DrugTypeOfMeasurement));
            cmd.Parameters.AddWithValue("@StandardChargeGross", ToDecimal(hospitalBody.StandardChargeGross));
            cmd.Parameters.AddWithValue("@StandardChargeDiscountedCash", ToDecimal(hospitalBody.StandardChargeDiscountedCash));
            cmd.Parameters.AddWithValue("@PayerName", ToString(hospitalBody.PayerName));
            cmd.Parameters.AddWithValue("@PlanName", ToString(hospitalBody.PlanName));
            cmd.Parameters.AddWithValue("@Modifiers", ToString(hospitalBody.Modifiers));
            cmd.Parameters.AddWithValue("@StandardChargeNegotiatedDollar", ToDecimal(hospitalBody.StandardChargeNegotiatedDollar));
            cmd.Parameters.AddWithValue("@StandardChargeNegotiatedAlgorithm", ToDecimal(hospitalBody.StandardChargeNegotiatedAlgorithm));
            cmd.Parameters.AddWithValue("@StandardChargeNegotiatedPercentage", ToDecimal(hospitalBody.StandardChargeNegotiatedPercentage));
            cmd.Parameters.AddWithValue("@StandardChargeMin", ToDecimal(hospitalBody.StandardChargeMin));
            cmd.Parameters.AddWithValue("@StandardChargeMax", ToDecimal(hospitalBody.StandardChargeMax));
            cmd.Parameters.AddWithValue("@CountOfComparedRates", ToInt(hospitalBody.CountOfComparedRates));
            cmd.Parameters.AddWithValue("@StandardChargeMethodology", ToString(hospitalBody.StandardChargeMethodology));
            cmd.Parameters.AddWithValue("@AdditionalGenericNotes", ToString(hospitalBody.AdditionalGenericNotes));
            cmd.Parameters.AddWithValue("@Footnote", ToString(hospitalBody.Footnote));

            cmd.ExecuteNonQuery();
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
        // sometimes code is in roman
        Map(m => m.Codei).Name("code|[i]");
        Map(m => m.CodeiType).Name("code|[i]|type");
        Map(m => m.Codeii).Name("code|[ii]");
        Map(m => m.CodeiiType).Name("code|[ii]|type");
        Map(m => m.Codeiii).Name("code|[iii]");
        Map(m => m.CodeiiiType).Name("code|[iii]|type");

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

public class HospitalDataMapWide : ClassMap<HospitalData>
{
    public HospitalDataMapWide()
    {
        int index = 0;
        Map(m => m.hospital_name).Index(index++);
        Map(m => m.last_updated_on).Index(index++);
        //Map(m => m.as_of_date).Index(index++);      // wide does not have as_of_date
        Map(m => m.version).Index(index++);
        Map(m => m.hospital_location).Index(index++);
        Map(m => m.hospital_address).Index(index++);
        Map(m => m.license_number).Index(index++);
        Map(m => m.financial_aid_policy).Index(7);
        Map(m => m.belief).Index(index++);
    }
}

public class HospitalDataMapTall : ClassMap<HospitalData>
{
    public HospitalDataMapTall()
    {
        int index = 0;
        Map(m => m.hospital_name).Index(index++);
        Map(m => m.last_updated_on).Index(index++);
        Map(m => m.as_of_date).Index(index++);
        Map(m => m.version).Index(index++);
        Map(m => m.hospital_location).Index(index++);
        Map(m => m.hospital_address).Index(index++);
        Map(m => m.license_number).Index(index++);
        Map(m => m.financial_aid_policy).Index(index++);
        Map(m => m.belief).Index(index++);
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
}
