namespace ParseHospitalFile
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            string file1 = @"C:\Data\Sinai-Hospital-of-Baltimore.csv";
            string file2 = @"C:\Data\UniversityofMarylandMedicalCenter.csv";
            string file3 = @"C:\Data\Holy-Cross-Hospital-Silver-Spring.csv";
            ParseHospitalFile parseHospitalFile = new ParseHospitalFile(file3);
            parseHospitalFile.Parse();
            Console.WriteLine("Done");
        }
    }
}
