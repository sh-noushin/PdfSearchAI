namespace InternalAIAssistant.Services;

public static class SimpleSearchService
{
    // Normalize text for better matching: lowercase and collapse whitespace
    private static string Normalize(string input)
    {
        if (input == null) return "";
        return string.Join(" ", input
            .ToLowerInvariant()
            .Split((char[])null, StringSplitOptions.RemoveEmptyEntries));
    }

    public static List<DocumentChunk> Search(List<DocumentChunk> chunks, string query, int topK = 3)
    {
        var normQuery = Normalize(query);
        var words = normQuery
            .Split(new[] { ' ', ',', '.', '?', '!', ';', ':' }, StringSplitOptions.RemoveEmptyEntries)
            .Where(w => w.Length > 2)
            .ToList();

        return chunks
            .Select(chunk =>
            {
                var normText = Normalize(chunk.Text);
                int score = words.Count(w => normText.Contains(w));
                return new { Chunk = chunk, Score = score };
            })
            .Where(x => x.Score > 0)
            .OrderByDescending(x => x.Score)
            .Take(topK)
            .Select(x => x.Chunk)
            .ToList();
    }
}