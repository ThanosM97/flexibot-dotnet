using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using System.Text;
using UglyToad.PdfPig;
using UglyToad.PdfPig.DocumentLayoutAnalysis.TextExtractor;

namespace ParserWorker.Services
{
    public class DocumentParser
    {
        public string ParseDocument(Stream fileStream, string fileName)
        {
            var extension = Path.GetExtension(fileName).ToLower();

            return extension switch
            {
                ".pdf" => ParsePdf(fileStream),
                ".docx" => ParseDocx(fileStream),
                _ => throw new NotSupportedException($"Unsupported file type: {extension}")
            };
        }

        private static string ParsePdf(Stream stream)
        {
            // Use PdfPig SDK to parse PDF
            using PdfDocument pdf = PdfDocument.Open(stream);

            var sb = new StringBuilder();
            foreach (var page in pdf.GetPages())
            {
                sb.AppendLine(ContentOrderTextExtractor.GetText(page));  // Append the text content of each page
            }

            return sb.ToString();  // Return the full text content of the pdf

        }

        private static string ParseDocx(Stream stream)
        {
            // Use Open XML SDK to parse DOCX
            using WordprocessingDocument doc = WordprocessingDocument.Open(stream, false);
            var body = doc.MainDocumentPart.Document.Body;

            // Get all paragraphs in the document
            var paragraphs = body.Elements<Paragraph>();

            var sb = new StringBuilder();
            foreach (var paragraph in paragraphs)
            {
                sb.AppendLine(paragraph.InnerText);  // Append the text content of each paragraph
            }

            return sb.ToString();  // Return the full text content of the document
        }
    }
}
