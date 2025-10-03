using InternalAIAssistant.Services;

public static class SemanticSearchService
{
    public static List<DocumentChunk> Search(List<DocumentChunk> chunks, float[] queryEmbedding, int topK = 3)
    {
        return chunks
            .Where(chunk => chunk.Embedding != null)
            .OrderByDescending(chunk => CosineSimilarity(chunk.Embedding, queryEmbedding))
            .Take(topK)
            .ToList();
    }

    private static float CosineSimilarity(float[] v1, float[] v2)
    {
        float dot = 0, norm1 = 0, norm2 = 0;
        for (int i = 0; i < v1.Length; i++)
        {
            dot += v1[i] * v2[i];
            norm1 += v1[i] * v1[i];
            norm2 += v2[i] * v2[i];
        }
        return dot / ((float)(Math.Sqrt(norm1) * Math.Sqrt(norm2)));
    }
}