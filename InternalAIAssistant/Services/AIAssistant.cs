using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using InternalAIAssistant.Helpers;
using OllamaSharp;
using OllamaSharp.Models;

namespace InternalAIAssistant.Services
{
    public enum SearchMode
    {
        Simple,
        Semantic
    }

    public class AIAssistant
    {
        private readonly DatabaseChunkService _databaseService;
        private readonly OllamaApiClient _client;
        
        /// <summary>
        /// Enable to write search debugging info to a file
        /// </summary>
        public bool EnableDebugLogging { get; set; } = false;

        public AIAssistant(DatabaseChunkService databaseService, string ollamaHost = "http://localhost:11434")
        {
            _databaseService = databaseService ?? throw new ArgumentNullException(nameof(databaseService));
            _client = new OllamaApiClient(ollamaHost);
        }

 public async Task<(string Answer, string Sources)> AskAsync(
    string question,
    SearchMode searchMode = SearchMode.Simple,
    float[]? queryEmbedding = null,
    int topK = 5,
    string model = "llama3")
        {
            // Always search chunks for every question
            var allChunks = await _databaseService.GetAllChunksAsync();

            Action<string>? debugOutput = EnableDebugLogging 
                ? msg => System.IO.File.AppendAllText("search-debug.log", $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {msg}\n")
                : null;

            // Limit chunk count and context size for performance
            int fastTopK = Math.Min(topK, 2); // Only use top 2 chunks for context
            List<DocumentChunk> topChunks = searchMode switch
            {
                SearchMode.Semantic when queryEmbedding != null =>
                    SemanticSearchService.Search(allChunks, queryEmbedding, fastTopK),
                _ =>
                    SimpleSearchService.Search(allChunks, question, fastTopK, debugOutput)
            };

            // Build context: concatenate only the actual chunk text, separated by ---
            string context = topChunks != null && topChunks.Any()
                ? string.Join("\n\n---\n\n", topChunks.Select(c => c.Text))
                : "";
            if (context.Length > 1500)
                context = context.Substring(0, 1500);

            string prompt;
            if (!string.IsNullOrWhiteSpace(context))
            {
                string langInstruction = "";
                // Improved heuristic: detect Persian, German, or English
                if (System.Text.RegularExpressions.Regex.IsMatch(question, "[\u0600-\u06FF]"))
                    langInstruction = "لطفاً به همان زبان سوال پاسخ دهید."; // Persian
                else if (System.Text.RegularExpressions.Regex.IsMatch(question, "[äöüß]|\b(der|die|das|und|ist|ein|eine|nicht|mit|auf|zu|für|im|dem|den|des|als|bei|nach|von|wie|man|aber|auch|nur|noch|schon|wird|werden|war|sein|hat|haben|dass|oder|wenn|was|wer|wo|wann|welche|dies|dieser|dieses|diese|ihre|ihren|ihrem|ihres|ihm|ihn|ihr|ihre|ihren|ihrem|ihres|wir|uns|unser|unserer|unserem|unseren|unseres|sie|ihnen|ihr|ihre|ihren|ihrem|ihres|es|am|im|um|zum|vom|beim|aus|ins|ans|aufs|über|unter|vor|hinter|zwischen|gegen|ohne|mit|durch|gegenüber|entlang|ab|an|auf|aus|bei|bis|durch|für|gegen|hinter|in|neben|über|unter|vor|zwischen)\b", System.Text.RegularExpressions.RegexOptions.IgnoreCase))
                    langInstruction = "Bitte antworte in deutscher Sprache."; // German
                else
                    langInstruction = "Please answer in English.";

                prompt =
                    "You are an expert software assistant. " +
                    "Answer the user's question using only the information below. " +
                    "If helpful, provide a brief definition, example, and list any key points. " +
                    "If there are code snippets in the context, include them in your answer. " +
                    "If the answer is not present, reply: 'I couldn't find the answer in your documents.'\n\n" +
                    "Always answer in the same language as the user's question. " + langInstruction + "\n\n" +
                    $"Context:\n{context}\n\nQuestion: {question}";
            }
            else
            {
                // No relevant chunk found, use helpful fallback
                return ("I couldn't find an answer in your documents. Please ask a question related to the content of your documents for best results.", string.Empty);
            }

            // Run LLM call on a background thread to avoid UI blocking
            string answer = await Task.Run(async () => {
                string result = "";
                await foreach (var chunk in _client.GenerateAsync(new GenerateRequest { Model = model, Prompt = prompt }))
                {
                    if (chunk?.Response != null)
                        result += chunk.Response;
                }
                return result.Trim();
            });

            // Sources: list document names with page numbers
            string sources = string.Empty;
            if (topChunks != null && topChunks.Any())
            {
                var sourcesByFile = topChunks
                    .GroupBy(c => c.FileName)
                    .Select(g => $"- {g.Key} (Pages: {string.Join(", ", g.Select(c => c.Page).Distinct().OrderBy(p => p))})")
                    .ToList();
                sources = string.Join("\n", sourcesByFile);
            }

            return (answer, sources);
        }
    private string GetFriendlyResponse(string question)
    {
        var lower = question.ToLowerInvariant();
        if (lower.Contains("hi") || lower.Contains("hello") || lower.Contains("hey"))
            return "Hello! How can I help you today?";
        if (lower.Contains("who are you") || lower.Contains("your name"))
            return "I'm your AI assistant, here to help you search and summarize your documents.";
        if (lower.Contains("what can you do"))
            return "I can answer questions and summarize information from your PDF documents. Just ask me about any topic covered in your files!";
        // Default friendly response
        return "I'm here to help! Ask me anything about your documents, or just chat.";
    }

    private bool IsGeneralQuestion(string question)
    {
        // Strict heuristic: only greetings and direct assistant questions
        var lower = question.ToLowerInvariant();
        if (lower.Contains("hi") || lower.Contains("hello") || lower.Contains("hey") || lower.Contains("good morning") || lower.Contains("good evening") || lower.Contains("good afternoon"))
            return true;
        if (lower.Contains("who are you") || lower.Contains("your name") || lower.Contains("are you assistant") || lower.Contains("what can you do") || lower.Contains("how do you work") || lower.Contains("what is your job") || lower.Contains("are you real") || lower.Contains("what are you"))
            return true;
        return false;
    }

    /// <summary>
    /// Summarizes or explains the entire document (or up to maxPages).
    /// </summary>
    /// <param name="fileName">The PDF or DOCX file name (must be in the chunks)</param>
    /// <param name="maxPages">Maximum number of pages to include as context (default 10)</param>
    /// <param name="model">LLM model to use</param>
    /// <returns>Tuple of (summary, sources)</returns>
    public async Task<(string Answer, string Sources)> SummarizeDocumentAsync(
        string fileName,
        int maxPages = 10,
        string model = "llama3")
    {
        // Find all chunks for the file from database
        var fileChunks = await _databaseService.GetChunksByFileAsync(fileName);
        fileChunks = fileChunks
            .OrderBy(c => c.Page)
            .Take(maxPages)
            .ToList();

        if (!fileChunks.Any())
            return ("No content found in the selected document.", "");

        var contextSections = fileChunks.Select(c =>
            $"[Source: {c.FileName}, page {c.Page}]\n{c.Text}").ToList();

        string context = string.Join("\n\n---\n\n", contextSections);

        string prompt =
            $"Summarize and explain the following document using only the provided context. " +
            $"If the context is too long, focus on the main ideas and key points.\n\nContext:\n{context}";

        var request = new GenerateRequest
        {
            Model = model,
            Prompt = prompt
        };

        string answer = "";
        await foreach (var chunk in _client.GenerateAsync(request))
        {
            if (chunk?.Response != null)
                answer += chunk.Response;
        }
        answer = string.IsNullOrWhiteSpace(answer) ? "No summary could be generated." : answer.Trim();
        string sources = string.Join("\n", fileChunks.Select(c => $"- {c.FileName}, page {c.Page}"));
        return (answer, sources);
    }
}

}