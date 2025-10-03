using System.Text.RegularExpressions;

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
            .Where(w => w.Length > 1) // Include 2-letter words (e.g., "AI", "DB")
            .ToList();

        if (!words.Any())
            return new List<DocumentChunk>();

        return chunks
            .Select(chunk =>
            {
                var normText = Normalize(chunk.Text);
                
                // Calculate TF (term frequency) score with word boundary matching
                double score = 0;
                foreach (var word in words)
                {
                    // Use word boundaries to match whole words only
                    var pattern = $@"\b{Regex.Escape(word)}\b";
                    var matches = Regex.Matches(normText, pattern, RegexOptions.IgnoreCase);
                    
                    // Score = number of occurrences of this word
                    score += matches.Count;
                }
                
                return new { Chunk = chunk, Score = score };
            })
            .Where(x => x.Score > 0)
            .OrderByDescending(x => x.Score)
            .Take(topK)
            .Select(x => x.Chunk)
            .ToList();
    }
}