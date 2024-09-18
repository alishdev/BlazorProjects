using System.Configuration;

namespace DataUploadTool
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            string connectionString = ConfigurationManager.ConnectionStrings["myConnectionString"].ConnectionString;
            AnthemUploadUtils.UploadFile1(connectionString, @"C:\Data\File1.csv");
        }
    }
}
