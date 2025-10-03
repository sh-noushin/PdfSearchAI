//using OllamaSharp;
//using System.Net.Http;
//using System.Text;
//using System.Text.Json;
//using System.Net.Http.Json;

//namespace InternalAIAssistant.Services;

//public class AIAssistant
//{
//    private readonly List<DocumentChunk> _chunks;
//    private readonly string _ollamaUrl = "http://localhost:11434/api/generate";
//    private readonly HttpClient _client = new HttpClient { Timeout = TimeSpan.FromMinutes(5) };

//    public AIAssistant(List<DocumentChunk> chunks)
//    {
//        _chunks = chunks ?? new List<DocumentChunk>();
//    }

//    public async Task<string> AskAsync(string question)
//    {
//        var topChunks = SimpleSearchService.Search(_chunks, question, topK: 3);
//        string context = string.Join("\n\n", topChunks.Select(c => c.Text));
//        if (context.Length > 1500)
//            context = context.Substring(0, 1500);

//        string prompt = !string.IsNullOrWhiteSpace(context)
//            ? $"Answer the following question based on the provided context.\nContext:\n{context}\n\nQuestion: {question}"
//            : question;

//        var payload = new { model = "mistral", prompt = prompt, stream = false };
//        try
//        {
//            var response = await _client.PostAsJsonAsync(_ollamaUrl, payload);
//            response.EnsureSuccessStatusCode();

//            var json = await response.Content.ReadAsStringAsync();
//            using var doc = JsonDocument.Parse(json);
//            string answer = doc.RootElement.GetProperty("response").GetString();
//            return string.IsNullOrWhiteSpace(answer)
//                ? "Sorry, I couldn't find an answer to your question."
//                : answer.Trim();
//        }
//        catch (TaskCanceledException)
//        {
//            return "Error: The request was canceled due to timeout. Try again with a shorter or simpler question/context.";
//        }
//        catch (Exception ex)
//        {
//            return $"Error: {ex.Message}";
//        }
//    }
//}



//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Threading.Tasks;
//using InternalAIAssistant.Helpers;
//using OllamaSharp;
//using OllamaSharp.Models;

//namespace InternalAIAssistant.Services
//{
//    public enum SearchMode
//    {
//        Simple,
//        Semantic
//    }

//    public class AIAssistant
//    {
//        private readonly List<DocumentChunk> _chunks;
//        private readonly OllamaApiClient _client;

//        public AIAssistant(List<DocumentChunk> chunks, string ollamaHost = "http://localhost:11434")
//        {
//            _chunks = chunks ?? throw new ArgumentNullException(nameof(chunks));
//            _client = new OllamaApiClient(ollamaHost);
//        }

//        public async Task<(string Answer, string Sources)> AskAsync(
//            string question,
//            SearchMode searchMode = SearchMode.Simple,
//            float[]? queryEmbedding = null,
//            int topK = 3,
//            string model = "llama3")
//        {
//            List<DocumentChunk> topChunks = searchMode switch
//            {
//                SearchMode.Semantic when queryEmbedding != null =>
//                    SemanticSearchService.Search(_chunks, queryEmbedding, topK),
//                _ =>
//                    SimpleSearchService.Search(_chunks, question, topK)
//            };

//            if (topChunks == null || !topChunks.Any())
//                return ("I couldn't find the answer in your documents.", string.Empty);

//            // Extract full pages for all found chunks (avoiding duplicates)
//            var pages = topChunks
//                .Select(c => (c.FileName, c.Page))
//                .Distinct()
//                .ToList();

//            var contextSections = pages
//                .Select(p =>
//                {
//                    string pageText = PdfHelper.ExtractPageText(p.FileName, p.Page);
//                    return $"[Source: {p.FileName}, page {p.Page}]\n{pageText}";
//                })
//                .ToList();

//            string context = string.Join("\n\n---\n\n", contextSections);

//            string prompt =
//                $"Only use the information below to answer the user's question. Do not use outside knowledge. " +
//                $"If you cannot find the answer, reply: 'I couldn't find the answer in the provided documents.'\n\n" +
//                $"Context:\n{context}\n\nQuestion: {question}";

//            var request = new GenerateRequest
//            {
//                Model = model,
//                Prompt = prompt
//            };

//            string answer = "";
//            await foreach (var chunk in _client.GenerateAsync(request))
//            {
//                if (chunk?.Response != null)
//                    answer += chunk.Response;
//            }
//            answer = string.IsNullOrWhiteSpace(answer) ? "I couldn't find the answer in your documents." : answer.Trim();
//            string sources = string.Join("\n", pages.Select(p => $"- {p.FileName}, page {p.Page}"));

//            return (answer, sources);
//        }
//    }
//}



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
        private readonly List<DocumentChunk> _chunks;
        private readonly OllamaApiClient _client;

        public AIAssistant(List<DocumentChunk> chunks, string ollamaHost = "http://localhost:11434")
        {
            _chunks = chunks ?? throw new ArgumentNullException(nameof(chunks));
            _client = new OllamaApiClient(ollamaHost);
        }

        /// <summary>
        /// Ask a question using either simple or semantic search.
        /// </summary>
        //    public async Task<(string Answer, string Sources)> AskAsync(
        //        string question,
        //        SearchMode searchMode = SearchMode.Simple,
        //        float[]? queryEmbedding = null,
        //        int topK = 3,
        //        string model = "llama3")
        //    {
        //        List<DocumentChunk> topChunks = searchMode switch
        //        {
        //            SearchMode.Semantic when queryEmbedding != null =>
        //                SemanticSearchService.Search(_chunks, queryEmbedding, topK),
        //            _ =>
        //                SimpleSearchService.Search(_chunks, question, topK)
        //        };

        //        if (topChunks == null || !topChunks.Any())
        //            return ("I couldn't find the answer in your documents.", string.Empty);

        //        // Extract full pages for all found chunks (avoiding duplicates)
        //        var pages = topChunks
        //            .Select(c => (c.FileName, c.Page))
        //            .Distinct()
        //            .ToList();

        //        var contextSections = pages
        //            .Select(p =>
        //            {
        //                string pageText = PdfHelper.ExtractPageText(p.FileName, p.Page);
        //                return $"[Source: {p.FileName}, page {p.Page}]\n{pageText}";
        //            })
        //            .ToList();

        //        string context = string.Join("\n\n---\n\n", contextSections);

        //        System.IO.File.WriteAllText("llm-debug-context.txt", context);

        //        //       string prompt =
        //        //$"Answer the user's question using only the information below. " +
        //        //$"If you are unsure, say so, but try your best to answer based on the context.\n\n" +
        //        //$"Context:\n{context}\n\nQuestion: {question}";

        //        string prompt =
        //"You are an expert software assistant. " +
        //"Answer the user's question using only the information below. " +
        //"If helpful, provide a brief definition, example, and list any key points. " +
        //"If the answer is not present, reply: 'I couldn't find the answer in your documents.'\n\n" +
        //$"Context:\n{context}\n\nQuestion: {question}";

        //        var request = new GenerateRequest
        //        {
        //            Model = model,
        //            Prompt = prompt
        //        };

        //        string answer = "";
        //        await foreach (var chunk in _client.GenerateAsync(request))
        //        {
        //            if (chunk?.Response != null)
        //                answer += chunk.Response;
        //        }
        //        answer = string.IsNullOrWhiteSpace(answer) ? "I couldn't find the answer in your documents." : answer.Trim();
        //        string sources = string.Join("\n", pages.Select(p => $"- {p.FileName}, page {p.Page}"));

        //        return (answer, sources);
        //    }




        //    public async Task<(string Answer, string Sources)> AskAsync(
        //string question,
        //SearchMode searchMode = SearchMode.Simple,
        //float[]? queryEmbedding = null,
        //int topK = 3,
        //string model = "llama3")
        //    {
        //        // Select top relevant document chunks
        //        List<DocumentChunk> topChunks = searchMode switch
        //        {
        //            SearchMode.Semantic when queryEmbedding != null =>
        //                SemanticSearchService.Search(_chunks, queryEmbedding, topK),
        //            _ =>
        //                SimpleSearchService.Search(_chunks, question, topK)
        //        };

        //        if (topChunks == null || !topChunks.Any())
        //            return ("I couldn't find the answer in your documents.", string.Empty);

        //        // Group by file name (no page numbers)
        //        var docNames = topChunks
        //            .Select(c => c.FileName)
        //            .Distinct()
        //            .ToList();

        //        // Build context: concatenate all unique relevant chunk texts (from all relevant files)
        //        var contextSections = topChunks
        //            .GroupBy(c => c.FileName)
        //            .Select(g => $"[Source: {g.Key}]\n" + string.Join("\n\n---\n\n", g.Select(c => c.Text)))
        //            .ToList();

        //        string context = string.Join("\n\n---\n\n", contextSections);

        //        // Prompt encourages code snippet extraction and concise, clear answers
        //        string prompt =
        //            "You are an expert software assistant. " +
        //            "Answer the user's question using only the information below. " +
        //            "If helpful, provide a brief definition, example, and list any key points. " +
        //            "If there are code snippets in the context, include them in your answer. " +
        //            "If the answer is not present, reply: 'I couldn't find the answer in your documents.'\n\n" +
        //            $"Context:\n{context}\n\nQuestion: {question}";

        //        var request = new GenerateRequest
        //        {
        //            Model = model,
        //            Prompt = prompt
        //        };

        //        string answer = "";
        //        await foreach (var chunk in _client.GenerateAsync(request))
        //        {
        //            if (chunk?.Response != null)
        //                answer += chunk.Response;
        //        }
        //        answer = string.IsNullOrWhiteSpace(answer) ? "I couldn't find the answer in your documents." : answer.Trim();

        //        // Sources: just document names, one per line
        //        string sources = string.Join("\n", docNames.Select(f => $"- {f}"));

        //        return (answer, sources);
        //    }




        public async Task<(string Answer, string Sources)> AskAsync(
    string question,
    SearchMode searchMode = SearchMode.Simple,
    float[]? queryEmbedding = null,
    int topK = 3,
    string model = "llama3")
        {
            // 1. Search top relevant chunks
            List<DocumentChunk> topChunks = searchMode switch
            {
                SearchMode.Semantic when queryEmbedding != null =>
                    SemanticSearchService.Search(_chunks, queryEmbedding, topK),
                _ =>
                    SimpleSearchService.Search(_chunks, question, topK)
            };

            if (topChunks == null || !topChunks.Any())
                return ("I couldn't find the answer in your documents.", string.Empty);

            // 2. Gather unique document names
            var docNames = topChunks
                .Select(c => c.FileName)
                .Distinct()
                .ToList();

            // 3. Build context grouped by file (all relevant text per file)
            var contextSections = topChunks
                .GroupBy(c => c.FileName)
                .Select(g => $"[Source: {g.Key}]\n" + string.Join("\n\n---\n\n", g.Select(c => c.Text)))
                .ToList();

            string context = string.Join("\n\n---\n\n", contextSections);

            // 4. Compose the LLM prompt
            string prompt =
                "You are an expert software assistant. " +
                "Answer the user's question using only the information below. " +
                "If helpful, provide a brief definition, example, and list any key points. " +
                "If there are code snippets in the context, include them in your answer. " +
                "If the answer is not present, reply: 'I couldn't find the answer in your documents.'\n\n" +
                $"Context:\n{context}\n\nQuestion: {question}";

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
            answer = string.IsNullOrWhiteSpace(answer) ? "I couldn't find the answer in your documents." : answer.Trim();

            // 5. Sources: list only document names, one per line
            string sources = string.Join("\n", docNames.Select(f => $"- {f}"));

            return (answer, sources);
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
            // Find all chunks for the file, ordered by page
            var fileChunks = _chunks
                .Where(c => c.FileName == fileName)
                .OrderBy(c => c.Page)
                .Take(maxPages)
                .ToList();

            if (!fileChunks.Any())
                return ("No content found in the selected document.", "");

            var contextSections = fileChunks.Select(c =>
                $"[Source: {c.FileName}, page {c.Page}]\n{PdfHelper.ExtractPageText(c.FileName, c.Page)}").ToList();

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