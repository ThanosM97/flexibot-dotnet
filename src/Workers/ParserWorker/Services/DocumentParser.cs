using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using System.Text;
using UglyToad.PdfPig;
using UglyToad.PdfPig.DocumentLayoutAnalysis.TextExtractor;

namespace ParserWorker.Services
{
    /// <summary>
    /// Provides functionality to parse documents from a stream.
    /// Supports PDF and DOCX formats.
    /// </summary>
    public class DocumentParser
    {
        /// <summary>
        /// Parses the content of a document from the given stream based on the file extension.
        /// </summary>
        /// <param name="fileStream">The stream containing the document data.</param>
        /// <param name="fileName">The name of the document file, used to determine the file extension.</param>
        /// <returns>A string containing the text content of the document.</returns>
        /// <exception cref="NotSupportedException">Thrown when the file type is not supported.</exception>
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

        /// <summary>
        /// Parses the text content of a PDF document from the given stream.
        /// </summary>
        /// <param name="stream">The stream containing the PDF data.</param>
        /// <returns>A string representing the full text content extracted from the PDF.</returns>
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

        /// <summary>
        /// Parses the text content of a DOCX document from the given stream.
        /// </summary>
        /// <param name="stream">The stream containing the DOCX data.</param>
        /// <returns>A string representing the full text content extracted from the DOCX document.</returns>
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
