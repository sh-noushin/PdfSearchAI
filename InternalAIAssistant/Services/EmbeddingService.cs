using OllamaSharp;
using System.Net.Http;
using System.Text.Json;
using System.Net.Http.Json;

namespace InternalAIAssistant.Services;

public class EmbeddingService
{
    private readonly string _ollamaUrl = "http://localhost:11434/api/embeddings";

    public async Task<float[]> GetEmbeddingAsync(string text)
    {
        var payload = new { model = "mistral", prompt = text };
        using var client = new HttpClient();
        var response = await client.PostAsJsonAsync(_ollamaUrl, payload);
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);
        var arr = doc.RootElement.GetProperty("embedding").EnumerateArray();
        var floats = new System.Collections.Generic.List<float>();
        foreach (var item in arr)
            floats.Add(item.GetSingle());
        return floats.ToArray();
    }
}
