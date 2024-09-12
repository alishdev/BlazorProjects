namespace ParseHospitalFile
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            ParseHospitalFile parseHospitalFile = new ParseHospitalFile(@"C:\Data\Sinai-Hospital-of-Baltimore.csv");
            parseHospitalFile.Parse();
            Console.WriteLine("Done");
            /*parseHospitalFile = new ParseHospitalFile(@"C:\Data\UniversityofMarylandMedicalCenter.csv");
            parseHospitalFile.Parse();
            Console.WriteLine("Done");*/
        }
    }
}
