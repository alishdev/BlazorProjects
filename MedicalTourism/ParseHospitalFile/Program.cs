namespace ParseHospitalFile
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            ParseHospitalFile parseHospitalFile = new ParseHospitalFile(@"C:\Data\Holy-Cross-Hospital-Silver-Spring.csv");
            //parseHospitalFile.Parse();
            Console.WriteLine("Done");
            parseHospitalFile = new ParseHospitalFile(@"C:\Data\UniversityofMarylandMedicalCenter.csv");
            parseHospitalFile.Parse();
            Console.WriteLine("Done");
        }
    }
}
