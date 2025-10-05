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
            var httpClient = new System.Net.Http.HttpClient
            {
                Timeout = TimeSpan.FromSeconds(120),
                BaseAddress = new Uri(ollamaHost)
            };
            _client = new OllamaApiClient(httpClient, ollamaHost);
        }

 public async Task<(string Answer, string Sources)> AskAsync(
    string question,
    SearchMode searchMode = SearchMode.Simple,
    float[]? queryEmbedding = null,
    int topK = 10,
    string model = "phi3")
        {
            // Always search chunks for every question
            var allChunks = await _databaseService.GetAllChunksAsync();

            Action<string>? debugOutput = EnableDebugLogging 
                ? msg => System.IO.File.AppendAllText("search-debug.log", $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {msg}\n")
                : null;

            // Limit chunk count and context size for performance
            int fastTopK = Math.Min(topK, 10); // Use up to 10 chunks for better answer quality
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
            if (context.Length > 2000)
                context = context.Substring(0, 2000);

            string prompt;
            if (!string.IsNullOrWhiteSpace(context))
            {
                string langInstruction = "";
                // Only detect German or default to English
                if (System.Text.RegularExpressions.Regex.IsMatch(question, "[äöüßÄÖÜ]") ||
                    System.Text.RegularExpressions.Regex.IsMatch(question, @"\b(der|die|das|und|ist|sind|ein|eine|nicht|mit|auf|zu|für|im|dem|den|des|als|bei|nach|von|wie|man|aber|auch|nur|noch|schon|wird|werden|wurde|war|sein|hat|haben|hatte|dass|oder|wenn|was|wer|wo|wann|welche|dies|dieser|dieses|diese|ihre|ihren|ihrem|ihres|ihm|ihn|wir|uns|unser|unserer|unserem|unseren|unseres|sie|ihnen|es|am|um|zum|vom|beim|aus|ins|ans|aufs|über|unter|vor|hinter|zwischen|gegen|ohne|durch)\b", System.Text.RegularExpressions.RegexOptions.IgnoreCase))
                {
                    langInstruction = "WICHTIG: Sie MÜSSEN ausschließlich auf Deutsch antworten. Verwenden Sie KEINE englischen Wörter oder Sätze. Antworten Sie NUR auf Deutsch."; // German
                }
                else
                {
                    langInstruction = "IMPORTANT: Please answer in English.";
                }

                prompt =
                    "SYSTEM: You are an expert assistant that answers questions based ONLY on the provided context below.\n" +
                    langInstruction + "\n\n" +
                    "CRITICAL INSTRUCTIONS:\n" +
                    "1. Answer ONLY using information from the context below\n" +
                    "2. Match the user's question language (German questions → German answers, English questions → English answers)\n" +
                    "3. Be comprehensive: use ALL relevant information from the context\n" +
                    "4. Structure your answer clearly with key points\n" +
                    "5. If the question is about concepts, definitions, or processes, explain them thoroughly\n" +
                    "6. Include specific details, examples, and explanations found in the context\n" +
                    "7. If multiple aspects are mentioned in context, cover all of them\n" +
                    "8. ONLY say 'I couldn't find the answer' if there is NO relevant information at all\n\n" +
                    $"CONTEXT:\n{context}\n\nQUESTION: {question}\n\nANSWER:";
            }
            else
            {
                // If any chunks matched, summarize them for the user
                if (topChunks != null && topChunks.Any())
                {
                    // Group chunks by file and summarize all relevant chunks together
                    var groupedChunks = topChunks
                        .GroupBy(c => c.FileName)
                        .Select(g => new {
                            FileName = g.Key,
                            Summary = string.Join("\n\n---\n\n", g.Select(c => c.Text))
                        })
                        .ToList();

                    string summary = string.Join("\n\n====\n\n", groupedChunks.Select(gc => $"{gc.FileName}:\n{gc.Summary}"));
                    string fallbackMsg;
                    if (System.Text.RegularExpressions.Regex.IsMatch(question, "[äöüßÄÖÜ]") ||
                        System.Text.RegularExpressions.Regex.IsMatch(question, @"\b(der|die|das|und|ist|sind|ein|eine|nicht|mit|auf|zu|für|im|dem|den|des|als|bei|nach|von|wie|man|aber|auch|nur|noch|schon|wird|werden|wurde|war|sein|hat|haben|hatte|dass|oder|wenn|was|wer|wo|wann|welche|dies|dieser|dieses|diese|ihre|ihren|ihrem|ihres|ihm|ihn|wir|uns|unser|unserer|unserem|unseren|unseres|sie|ihnen|es|am|um|zum|vom|beim|aus|ins|ans|aufs|über|unter|vor|hinter|zwischen|gegen|ohne|durch)\b", System.Text.RegularExpressions.RegexOptions.IgnoreCase))
                    {
                        fallbackMsg = $"Ich habe die folgenden relevanten Informationen in Ihren Dokumenten gefunden:\n\n{summary}";
                    }
                    else
                    {
                        fallbackMsg = $"I found the following relevant information in your documents:\n\n{summary}";
                    }
                    return (fallbackMsg, string.Join("\n", topChunks.Select(c => $"- {c.FileName}").Distinct()));
                }
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
                    .Select(c => $"- {c.FileName}")
                    .Distinct()
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
        string model = "phi3")
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