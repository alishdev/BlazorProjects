namespace ParseCDS;

class Program
{
    static void Main(string[] args)
    {
        /*var parser = new CDSParser();
        var c1Data = parser.ParsePDFAndSerializeC1("/Users/macmyths/Desktop/ParsePDF/CDS/boston-u-cds-2023.pdf");
        parser.SerializeToJson(c1Data, "c1_data_output.json");*/
        
        var parser = new PDFParser();
        var c1Data = parser.ParsePDFAndSerializeC1("/Users/macmyths/Desktop/ParsePDF/CDS/boston-u-cds-2023.pdf");
        parser.SerializeToJson(c1Data, "c1_data_image.json");
    }
}