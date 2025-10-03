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

    /// <summary>
    /// Search for relevant chunks using TF (term frequency) scoring with word boundary matching
    /// </summary>
    /// <param name="chunks">All available document chunks</param>
    /// <param name="query">User's search query</param>
    /// <param name="topK">Number of top-scoring chunks to return</param>
    /// <param name="debugOutput">Optional: outputs debug information about scoring</param>
    /// <returns>List of top-K most relevant chunks</returns>
    public static List<DocumentChunk> Search(List<DocumentChunk> chunks, string query, int topK = 3, Action<string>? debugOutput = null)
    {
        var normQuery = Normalize(query);
        var words = normQuery
            .Split(new[] { ' ', ',', '.', '?', '!', ';', ':' }, StringSplitOptions.RemoveEmptyEntries)
            .Where(w => w.Length > 1) // Include 2-letter words (e.g., "AI", "DB")
            .ToList();

        if (!words.Any())
            return new List<DocumentChunk>();

        debugOutput?.Invoke($"Search query: '{query}' => {words.Count} search terms: [{string.Join(", ", words)}]");

        var scoredChunks = chunks
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
            .ToList();

        debugOutput?.Invoke($"Found {scoredChunks.Count} chunks with matches, returning top {Math.Min(topK, scoredChunks.Count)}");
        
        if (debugOutput != null && scoredChunks.Any())
        {
            foreach (var item in scoredChunks.Take(topK))
            {
                var preview = item.Chunk.Text.Length > 100 ? item.Chunk.Text.Substring(0, 100) + "..." : item.Chunk.Text;
                debugOutput($"  - Score: {item.Score:F1}, File: {item.Chunk.FileName}, Page: {item.Chunk.Page}, Preview: {preview}");
            }
        }

        return scoredChunks
            .Take(topK)
            .Select(x => x.Chunk)
            .ToList();
    }
}