using System.IO;
using UglyToad.PdfPig;
using Xceed.Words.NET;

namespace InternalAIAssistant.Helpers;

public static class DocumentParser
{
    public static List<(string Text, string FileName, int Page)> ParsePdf(string filePath)
    {
        var results = new List<(string, string, int)>();
        using (var document = PdfDocument.Open(filePath))
        {
            int pageNum = 1;
            foreach (var page in document.GetPages())
            {
                string text = page.Text;
                results.Add((text, Path.GetFileName(filePath), pageNum));
                pageNum++;
            }
        }
        return results;
    }

    public static List<(string Text, string FileName)> ParseDocx(string filePath)
    {
        var results = new List<(string, string)>();
        using (var document = DocX.Load(filePath))
        {
            results.Add((document.Text, Path.GetFileName(filePath)));
        }
        return results;
    }
}
