using DocumentFormat.OpenXml.Packaging;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Parser;
using iText.Kernel.Pdf.Canvas.Parser.Listener;
using System.Text;

namespace LMS.Infrastructure.Services;

public class TextExtractionService
{
    public string ExtractText(string filePath)
    {
        var extension = Path.GetExtension(filePath).ToLowerInvariant();
        return extension switch
        {
            ".pdf" => ExtractFromPdf(filePath),
            ".docx" => ExtractFromWord(filePath),
            ".txt" => File.ReadAllText(filePath),
            _ => throw new NotSupportedException($"Extension {extension} is not supported.")
        };
    }

    private string ExtractFromPdf(string filePath)
    {
        var sb = new StringBuilder();
        using var pdfReader = new PdfReader(filePath);
        using var pdfDocument = new PdfDocument(pdfReader);
        for(int i = 1; i <= pdfDocument.GetNumberOfPages(); i++)
        {
            var strategy = new LocationTextExtractionStrategy();
            var text = PdfTextExtractor.GetTextFromPage(pdfDocument.GetPage(i), strategy);
            sb.AppendLine(text);
        }
        return sb.ToString();
    }

    private string ExtractFromWord(string filePath)
    {
        using var doc = WordprocessingDocument.Open(filePath, false);
        var body = doc.MainDocumentPart?.Document.Body;
        return body?.InnerText ?? string.Empty;
    }
}
