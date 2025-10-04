using PdfChunkService.Configuration;
using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using PdfChunkService.Data;
using PdfChunkService.Models;
using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;

namespace PdfChunkService.Services;

public interface IPdfProcessingService
{
    Task<bool> ProcessDirectoryAsync(string directoryPath, int chunkSize);
    Task<bool> ProcessFileAsync(string filePath, int chunkSize);
}

public class PdfProcessingService : IPdfProcessingService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<PdfProcessingService> _logger;
    private readonly ServiceConfiguration _config;

    public PdfProcessingService(IServiceProvider serviceProvider, ILogger<PdfProcessingService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        // Try to get ServiceConfiguration from DI
        _config = serviceProvider.GetService(typeof(ServiceConfiguration)) as ServiceConfiguration
            ?? throw new InvalidOperationException("ServiceConfiguration not found in DI container");
    }

    public async Task<bool> ProcessDirectoryAsync(string directoryPath, int chunkSize)
    {
        try
        {
            if (!Directory.Exists(directoryPath))
            {
                _logger.LogError("Directory does not exist: {DirectoryPath}", directoryPath);
                return false;
            }

            var pdfFiles = Directory.GetFiles(directoryPath, "*.pdf", SearchOption.AllDirectories);
            _logger.LogInformation("Found {FileCount} PDF files in directory: {DirectoryPath}", pdfFiles.Length, directoryPath);

            foreach (var filePath in pdfFiles)
            {
                _logger.LogInformation($"Processing PDF: {filePath}");
                try
                {
                    var result = await ProcessFileAsync(filePath, chunkSize);
                    if (result)
                    {
                        _logger.LogInformation($"Successfully processed: {filePath}");
                    }
                    else
                    {
                        _logger.LogWarning($"Skipped or failed: {filePath}");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Exception while processing: {filePath}");
                }
            }

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to process directory: {DirectoryPath}", directoryPath);
            return false;
        }
    }

    public async Task<bool> ProcessFileAsync(string filePath, int chunkSize)
    {
        try
        {
            if (!File.Exists(filePath))
            {
                _logger.LogError("File does not exist: {FilePath}", filePath);
                return false;
            }

            using var scope = _serviceProvider.CreateScope();
            var dbContext = new PdfChunkDbContext(PdfChunkDbContext.GetOptions(_config.ConnectionString));

            var fileInfo = new FileInfo(filePath);
            var fileHash = await CalculateFileHashAsync(filePath);

            // Check if file already exists in database
            var existingFile = await dbContext.Files
                .FirstOrDefaultAsync(f => f.FilePath == filePath || f.FileHash == fileHash);

            if (existingFile != null)
            {
                // Check if file has been modified
                if (existingFile.LastModified >= fileInfo.LastWriteTime)
                {
                    _logger.LogDebug("File already processed and up to date: {FilePath}", filePath);
                    return true;
                }

                // File has been modified, remove old data
                _logger.LogInformation("File has been modified, removing old data: {FilePath}", filePath);
                dbContext.Files.Remove(existingFile);
                await dbContext.SaveChangesAsync();
            }

            // Extract text from PDF
            var textContent = ExtractTextFromPdf(filePath);
            if (string.IsNullOrWhiteSpace(textContent))
            {
                _logger.LogWarning("No text content found in PDF: {FilePath}", filePath);
                return false;
            }

            // Create file entity
            var fileEntity = new FileEntity
            {
                FileName = fileInfo.Name,
                FilePath = filePath,
                CreatedAt = DateTime.UtcNow,
                LastModified = fileInfo.LastWriteTime,
                FileSize = fileInfo.Length,
                FileHash = fileHash
            };

            dbContext.Files.Add(fileEntity);
            await dbContext.SaveChangesAsync();

            // Create chunks
            var chunks = CreateChunks(textContent, chunkSize);
            for (int i = 0; i < chunks.Count; i++)
            {
                var chunkEntity = new ChunkEntity
                {
                    FileId = fileEntity.Id,
                    Content = chunks[i],
                    ChunkIndex = i,
                    PageNumber = i + 1, // Simplified page numbering
                    CreatedAt = DateTime.UtcNow
                };

                dbContext.Chunks.Add(chunkEntity);
            }

            await dbContext.SaveChangesAsync();
            _logger.LogInformation("Successfully processed file: {FilePath} with {ChunkCount} chunks", filePath, chunks.Count);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to process file: {FilePath}", filePath);
            return false;
        }
    }

    private string ExtractTextFromPdf(string filePath)
    {
        try
        {
            var textContent = new StringBuilder();
            // Use PdfPig for proper text extraction
            using (var document = UglyToad.PdfPig.PdfDocument.Open(filePath))
            {
                foreach (var page in document.GetPages())
                {
                    var text = page.Text;
                    textContent.AppendLine(text);
                }
            }
            return textContent.ToString();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to extract text from PDF: {FilePath}", filePath);
            return string.Empty;
        }
    }

    private List<string> CreateChunks(string text, int chunkSize)
    {
        var chunks = new List<string>();
        
        for (int i = 0; i < text.Length; i += chunkSize)
        {
            var chunk = text.Substring(i, Math.Min(chunkSize, text.Length - i));
            chunks.Add(chunk);
        }

        return chunks;
    }

    private async Task<string> CalculateFileHashAsync(string filePath)
    {
        using var md5 = MD5.Create();
        using var stream = File.OpenRead(filePath);
        var hash = await Task.Run(() => md5.ComputeHash(stream));
        return Convert.ToBase64String(hash);
    }
}