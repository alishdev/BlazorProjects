using MySql.Data.MySqlClient;

namespace DataUploadTool;

internal class File1Model
{
    public string EIN { get; set; }
    public string CompanyName { get; set; }
    public List<string> Plans { get; set; }
}

internal class AnthemUploadUtils
{
    public static void UploadFile1(string connectionString, string file1path)
    {

        List<File1Model> file1Data = ReadFile1(file1path);

        // Create a MySqlConnection object
        using (MySqlConnection connection = new MySqlConnection(connectionString))
        {
            try
            {
                connection.Open();


                using (MySqlTransaction transaction = connection.BeginTransaction())
                {
                    // first prepare cmdCompany command
                    MySqlCommand cmdCompany = connection.CreateCommand();
                    cmdCompany.Transaction = transaction;

                    // Set the SQL query to insert the File1Model data into the database
                    cmdCompany.CommandText = "INSERT INTO Companies (EIN, CompanyName) VALUES (@EIN, @CompanyName)";

                    // Add parameters to the SQL query
                    cmdCompany.Parameters.AddWithValue("@EIN", "example_ein");
                    cmdCompany.Parameters.AddWithValue("@CompanyName", "example_company_name");

                    MySqlCommand cmdPlans = connection.CreateCommand();
                    cmdPlans.Transaction = transaction;

                    // Set the SQL query to insert the File1Model data into the database
                    cmdPlans.CommandText = "INSERT INTO CompanyPlans (EIN, PlanName) VALUES (@EIN, @PlanName)";

                    // Add parameters to the SQL query
                    cmdPlans.Parameters.AddWithValue("@EIN", "example_ein");
                    cmdPlans.Parameters.AddWithValue("@PlanName", "example_company_name");

                    foreach (File1Model file1 in file1Data)
                    {
                        // Set the parameter values for the current File1Model object
                        cmdCompany.Parameters["@EIN"].Value = file1.EIN;
                        cmdCompany.Parameters["@CompanyName"].Value = file1.CompanyName;

                        cmdCompany.ExecuteNonQuery();

                        foreach (string plan in file1.Plans)
                        {
                            // Set the parameter values for the current File1Model object
                            cmdPlans.Parameters["@EIN"].Value = file1.EIN;
                            cmdPlans.Parameters["@PlanName"].Value = plan;

                            cmdPlans.ExecuteNonQuery();
                        }
                    }
                    transaction.Commit();
                }

                connection.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
            }
        }
    }

    public static List<File1Model> ReadFile1(string file1path)
    {
        // Read the data from the file located at file1path
        string line;
        string lastPlan = "";
        string lastEIN = "";
        string lastCompanyName = "";
        File1Model file1 = null;
        List<File1Model> file1Data = new List<File1Model>();
        int lineIndex = 0;
        bool foundShiji = false;
        using (StreamReader reader = new StreamReader(file1path))
        {
            while ((line = reader.ReadLine()) != null)
            {
                string[] values = line.Split(',');
                if (values.Length < 2)
                    throw new Exception($"Invalid line format: {line}");

                string tempEIN = values[1];
                string tempPlan = values[2];
                string tempCompanyName = values[0];

                if (tempEIN != lastEIN)
                {
                    file1 = file1Data.Find(x => x.EIN == tempEIN);
                    if (file1Data.Find(x => x.EIN == tempEIN) == null)
                    {
                        file1 = new File1Model()
                        {
                            CompanyName = values[0],
                            EIN = values[1],
                            Plans = new() { tempPlan }
                        };
                        file1Data.Add(file1);
                    }
                    if (lastCompanyName.Contains("Shiji", StringComparison.OrdinalIgnoreCase))
                        foundShiji = true;
                }
                else if (lastPlan != tempPlan)
                {
                    file1.Plans.Add(tempPlan);
                }

                lastEIN = tempEIN;
                lastPlan = tempPlan;
                lastCompanyName = tempCompanyName;

                if ((++lineIndex % 10000) == 0)
                    Console.Write(".");
                // break for testing
                //if (lineIndex > 1000)
                //    break;

                if (foundShiji)
                {
                    Console.WriteLine($"Found Shiji at line {lineIndex}");
                    break;
                }
            }
        }

        Console.WriteLine($"Read {file1Data.Count} records from file1");

        return file1Data;
    }
}
