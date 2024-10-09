using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Net.Mime;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using PdfiumViewer;
using Tesseract;

namespace ParseCDS;

public class PDFParser
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
        using (var document = PdfDocument.Load(pdfPath))
        {
            var sb = new System.Text.StringBuilder();

            for (int i = 0; i < document.PageCount; i++)
            {
                using (var image = document.Render(i, 300, 300, false))
                {
                    string pageText = PerformOCR(image);
                    sb.AppendLine(pageText);

                    if (pageText.Contains("C1. First-time, first-year (freshman) students"))
                    {
                        // We've found the C1 section, no need to process more pages
                        break;
                    }
                }
            }

            return sb.ToString();
        }
    }

    private string PerformOCR(Image image)
    {
        System.Diagnostics.Debug.WriteLine(Path.GetFullPath("./tessdata"));
        using (var engine = new TesseractEngine(@"./tessdata", "eng", EngineMode.Default))
        {
            using (var pix = PixConverter.ToPix((Bitmap)image))
            {
                using (var page = engine.Process(pix))
                {
                    return page.GetText();
                }
            }
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