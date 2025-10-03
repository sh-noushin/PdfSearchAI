# Troubleshooting Guide

## AI Giving Wrong Answers or Finding Wrong PDF Files

If the AI assistant is giving incorrect answers or citing the wrong PDF files, follow these debugging steps:

### 1. Enable Search Debug Logging

The search algorithm can now output detailed debugging information to help you understand which chunks are being matched and why.

**To enable debug logging:**

In `MainWindow.xaml.cs`, after creating the AIAssistant, add:

```csharp
var assistant = new AIAssistant(databaseService);
assistant.EnableDebugLogging = true;  // Add this line
```

This will create a `search-debug.log` file in the application directory showing:
- Which search terms were extracted from your query
- How many chunks matched
- The score, filename, and page number of each matched chunk
- A preview of the matched text

### 2. Review the Debug Log

Example debug output:
```
[2024-01-15 10:30:45] Search query: 'What is dependency injection?' => 3 search terms: [what, dependency, injection]
[2024-01-15 10:30:45] Found 12 chunks with matches, returning top 5
[2024-01-15 10:30:45]   - Score: 8.0, File: CSharp-Guide.pdf, Page: 15, Preview: Dependency injection is a design pattern...
[2024-01-15 10:30:45]   - Score: 6.0, File: Design-Patterns.pdf, Page: 42, Preview: The injection pattern allows...
```

### 3. Verify Database Content

Check that the PdfChunkService has properly processed your PDF files:

```csharp
var (fileCount, chunkCount) = await databaseService.GetStatisticsAsync();
Console.WriteLine($"Database has {chunkCount} chunks from {fileCount} files");
```

### 4. Understanding Search Improvements

The search algorithm has been improved with:

1. **Word Boundary Matching**: Now uses regex `\b` to match whole words only
   - Before: "test" matched "latest", "fastest", "contest"
   - After: "test" only matches "test"

2. **Term Frequency (TF) Scoring**: Counts how many times each word appears
   - Before: A word appearing once scored the same as appearing 100 times
   - After: More occurrences = higher score

3. **Better Context Coverage**: Default increased from 1 to 5 top chunks
   - Before: Only used the single highest-scoring chunk (might be wrong)
   - After: Uses top 5 chunks for more comprehensive context

4. **Improved Source Tracking**: Shows specific page numbers
   - Example: `file.pdf (Pages: 1, 3, 5)` instead of just `file.pdf`

### 5. Common Issues and Solutions

**Issue**: Search finds wrong files
- **Cause**: Query words are too common (e.g., "the", "is", "and")
- **Solution**: Make your query more specific with unique keywords

**Issue**: No results found
- **Cause**: Database might not have the documents processed yet
- **Solution**: Verify PdfChunkService is running and has processed your PDFs

**Issue**: Results from old/deleted files
- **Cause**: Database not updated after file deletion
- **Solution**: Restart PdfChunkService to rescan the directory

### 6. Advanced: Adjust Search Parameters

You can modify search behavior in code:

```csharp
// Increase number of chunks used for context (default is 5)
var (answer, sources) = await assistant.AskAsync(question, topK: 10);

// Adjust context length limit (default is 2000 characters)
// Edit AIAssistant.cs line ~344
```

### 7. Need More Help?

If issues persist:
1. Check the `search-debug.log` for scoring information
2. Verify the database contains the expected chunks
3. Ensure PdfChunkService is running and monitoring the correct directory
4. Check that PDF files are being correctly parsed (not scanned images without OCR)
