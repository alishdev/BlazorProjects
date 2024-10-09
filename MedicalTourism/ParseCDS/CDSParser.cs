using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using iTextSharp.text.pdf;
using iTextSharp.text.pdf.parser;
using Newtonsoft.Json;

namespace ParseCDS;

// class created by Claude
public class CDSParser
{
    public class C1Data
    {
        public string Category { get; set; }
        public int? ApplicantsMen { get; set; }
        public int? ApplicantsWomen { get; set; }
        public int? ApplicantsTotal { get; set; }
        public int? AdmittedMen { get; set; }
        public int? AdmittedWomen { get; set; }
        public int? AdmittedTotal { get; set; }
        public int? EnrolledMen { get; set; }
        public int? EnrolledWomen { get; set; }
        public int? EnrolledTotal { get; set; }
    }

    public List<C1Data> ParsePDFAndSerializeC1(string pdfPath)
    {
        string pdfText = ExtractTextFromPDF(pdfPath);
        return ParseC1Data(pdfText);
    }

    private string ExtractTextFromPDF(string pdfPath)
    {
        using (PdfReader reader = new PdfReader(pdfPath))
        {
            StringWriter output = new StringWriter();

            for (int i = 1; i <= reader.NumberOfPages; i++)
            {
                string text = PdfTextExtractor.GetTextFromPage(reader, i);
                output.WriteLine(text);
            }

            return output.ToString();
        }
    }

    private List<C1Data> ParseC1Data(string pdfText)
    {
        List<C1Data> c1DataList = new List<C1Data>();

        // Find the C1 section
        int startIndex = pdfText.IndexOf("C1. First-time, first-year (freshman) students");
        if (startIndex == -1) return c1DataList;

        int endIndex = pdfText.IndexOf("C2.", startIndex);
        if (endIndex == -1) endIndex = pdfText.Length;

        string c1Section = pdfText.Substring(startIndex, endIndex - startIndex);

        // Use regex to find and parse the data
        string pattern = @"(\w+(\s+\w+)*)\s+(\d+)\s+(\d+)\s+(\d+)\s+(\d+)\s+(\d+)\s+(\d+)\s+(\d+)\s+(\d+)\s+(\d+)";
        MatchCollection matches = Regex.Matches(c1Section, pattern);

        foreach (Match match in matches)
        {
            c1DataList.Add(new C1Data
            {
                Category = match.Groups[1].Value.Trim(),
                ApplicantsMen = int.Parse(match.Groups[3].Value),
                ApplicantsWomen = int.Parse(match.Groups[4].Value),
                ApplicantsTotal = int.Parse(match.Groups[5].Value),
                AdmittedMen = int.Parse(match.Groups[6].Value),
                AdmittedWomen = int.Parse(match.Groups[7].Value),
                AdmittedTotal = int.Parse(match.Groups[8].Value),
                EnrolledMen = int.Parse(match.Groups[9].Value),
                EnrolledWomen = int.Parse(match.Groups[10].Value),
                EnrolledTotal = int.Parse(match.Groups[11].Value)
            });
        }

        return c1DataList;
    }

    public void SerializeToJson(List<C1Data> data, string outputPath)
    {
        string json = JsonConvert.SerializeObject(data, Formatting.Indented);
        File.WriteAllText(outputPath, json);
    }
}