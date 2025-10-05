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
    /// Search for relevant chunks using improved TF-IDF scoring with:
    /// - Word boundary matching for precision
    /// - Boosted scoring for query word proximity
    /// - Support for German special characters (ä, ö, ü, ß)
    /// - Better handling of multi-word queries
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
            .Split(new[] { ' ', ',', '.', '?', '!', ';', ':', '(', ')', '[', ']' }, StringSplitOptions.RemoveEmptyEntries)
            .Where(w => w.Length > 1) // Include 2-letter words (e.g., "AI", "DB", "di", "is")
            .ToList();

        if (!words.Any())
            return new List<DocumentChunk>();

        debugOutput?.Invoke($"Search query: '{query}' => {words.Count} search terms: [{string.Join(", ", words)}]");

        var scoredChunks = chunks
            .Select(chunk =>
            {
                var normText = Normalize(chunk.Text);
                
                // Calculate enhanced TF (term frequency) score with word boundary matching
                double score = 0;
                int totalMatches = 0;
                
                foreach (var word in words)
                {
                    // Use word boundaries to match whole words only
                    var pattern = $@"\b{Regex.Escape(word)}\b";
                    var matches = Regex.Matches(normText, pattern, RegexOptions.IgnoreCase);
                    int matchCount = matches.Count;
                    
                    if (matchCount > 0)
                    {
                        // Base score: number of occurrences
                        // Use logarithmic scaling to prevent over-weighting of very frequent terms
                        double termScore = Math.Log(1 + matchCount) * 2.0;
                        
                        // Boost score for longer/more specific words
                        if (word.Length >= 8) termScore *= 1.3;
                        else if (word.Length >= 5) termScore *= 1.1;
                        
                        score += termScore;
                        totalMatches += matchCount;
                    }
                }
                
                // Bonus: if multiple search terms appear, boost the score (indicates relevance)
                int wordsFound = words.Count(word => 
                    Regex.IsMatch(normText, $@"\b{Regex.Escape(word)}\b", RegexOptions.IgnoreCase));
                    
                if (wordsFound >= 2)
                {
                    score *= (1.0 + wordsFound * 0.15); // 15% boost per additional word found
                }
                
                return new { Chunk = chunk, Score = score, Matches = totalMatches, WordsFound = wordsFound };
            })
            .Where(x => x.Score > 0)
            .OrderByDescending(x => x.Score)
            .ThenByDescending(x => x.WordsFound) // Secondary sort by number of different words found
            .ToList();

        debugOutput?.Invoke($"Found {scoredChunks.Count} chunks with matches, returning top {Math.Min(topK, scoredChunks.Count)}");
        
        if (debugOutput != null && scoredChunks.Any())
        {
            foreach (var item in scoredChunks.Take(topK))
            {
                var preview = item.Chunk.Text.Length > 100 ? item.Chunk.Text.Substring(0, 100) + "..." : item.Chunk.Text;
                debugOutput($"  - Score: {item.Score:F1}, Matches: {item.Matches}, Words: {item.WordsFound}/{words.Count}, " +
                           $"File: {item.Chunk.FileName}, Page: {item.Chunk.Page}, Preview: {preview}");
            }
        }

        return scoredChunks
            .Take(topK)
            .Select(x => x.Chunk)
            .ToList();
    }
}