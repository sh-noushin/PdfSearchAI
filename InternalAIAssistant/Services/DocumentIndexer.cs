using System.IO;

namespace InternalAIAssistant.Services;

public class DocumentChunk
{
    public string Text { get; set; }
    public string FileName { get; set; }
    public int Page { get; set; }
    public float[] Embedding { get; set; }
}

public class DocumentIndexer
{
    public List<DocumentChunk> Chunks { get; set; } = new();

    public void IndexFolder(string folderPath)
    {
        Chunks.Clear();
        foreach (var file in Directory.GetFiles(folderPath, "*.pdf"))
        {
            var pdfChunks = Helpers.DocumentParser.ParsePdf(file); // Should return (string Text, string FileName, int Page)
            foreach (var chunk in pdfChunks)
                Chunks.Add(new DocumentChunk { Text = chunk.Text, FileName = chunk.FileName, Page = chunk.Page });
        }
        foreach (var file in Directory.GetFiles(folderPath, "*.docx"))
        {
            var docxChunks = Helpers.DocumentParser.ParseDocx(file); // Should return (string Text, string FileName)
            foreach (var chunk in docxChunks)
                Chunks.Add(new DocumentChunk { Text = chunk.Text, FileName = chunk.FileName, Page = 1 });
        }
    }
}