using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using PdfChunkService.Data;
using PdfChunkService.Models;
using Serilog;
using UglyToad.PdfPig;

namespace PdfChunkService.Services;

public class PdfProcessingService
{
    private readonly PdfChunkDbContext _context;
    private readonly string _documentsDirectory;

    public PdfProcessingService(PdfChunkDbContext context, string documentsDirectory)
    {
        _context = context;
        _documentsDirectory = documentsDirectory;
    }

    /// <summary>
    /// Process all PDF files in the configured directory
    /// </summary>
    public async Task ProcessAllPdfsAsync()
    {
        if (!Directory.Exists(_documentsDirectory))
        {
            Log.Warning("Documents directory does not exist: {Directory}", _documentsDirectory);
            return;
        }

        var pdfFiles = Directory.GetFiles(_documentsDirectory, "*.pdf", SearchOption.AllDirectories);
        Log.Information("Found {Count} PDF files to process", pdfFiles.Length);

        foreach (var pdfFile in pdfFiles)
        {
            try
            {
                await ProcessPdfFileAsync(pdfFile);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error processing PDF file: {FilePath}", pdfFile);
            }
        }

        await _context.SaveChangesAsync();
        Log.Information("PDF processing completed");
    }

    /// <summary>
    /// Process a single PDF file
    /// </summary>
    private async Task ProcessPdfFileAsync(string filePath)
    {
        var fileInfo = new FileInfo(filePath);
        if (!fileInfo.Exists)
        {
            Log.Warning("File does not exist: {FilePath}", filePath);
            return;
        }

        // Calculate file hash
        var fileHash = CalculateFileHash(filePath);
        var fileName = fileInfo.Name;

        // Check if file already exists in database and hasn't changed
        var existingFile = await _context.Files
            .Include(f => f.Chunks)
            .FirstOrDefaultAsync(f => f.FilePath == filePath);

        if (existingFile != null)
        {
            // Check if file has been modified
            if (existingFile.FileHash == fileHash && 
                existingFile.LastModified == fileInfo.LastWriteTimeUtc)
            {
                Log.Debug("File hasn't changed, skipping: {FileName}", fileName);
                return;
            }

            // File has changed, remove existing chunks
            Log.Information("File has changed, reprocessing: {FileName}", fileName);
            _context.Chunks.RemoveRange(existingFile.Chunks);
            existingFile.Chunks.Clear();
        }
        else
        {
            // Create new file entity
            existingFile = new FileEntity
            {
                FileName = fileName,
                FilePath = filePath,
                FileHash = fileHash,
                FileSize = fileInfo.Length,
                CreatedAt = DateTime.UtcNow,
                LastModified = fileInfo.LastWriteTimeUtc
            };
            _context.Files.Add(existingFile);
            await _context.SaveChangesAsync(); // Save to get the FileId
        }

        // Update file metadata
        existingFile.FileHash = fileHash;
        existingFile.LastModified = fileInfo.LastWriteTimeUtc;
        existingFile.FileSize = fileInfo.Length;

        // Extract text from PDF and create chunks
        var chunks = ExtractTextFromPdf(filePath, fileName);
        
        Log.Information("Extracted {Count} chunks from {FileName}", chunks.Count, fileName);

        // Create chunk entities
        int chunkIndex = 0;
        foreach (var (text, pageNumber) in chunks)
        {
            var chunkEntity = new ChunkEntity
            {
                FileId = existingFile.Id,
                Content = text,
                ChunkIndex = chunkIndex++,
                PageNumber = pageNumber,
                CreatedAt = DateTime.UtcNow,
                File = existingFile
            };
            _context.Chunks.Add(chunkEntity);
        }

        await _context.SaveChangesAsync();
        Log.Information("Successfully processed {FileName} with {ChunkCount} chunks", fileName, chunks.Count);
    }

    /// <summary>
    /// Extract text from PDF file, one chunk per page
    /// </summary>
    private List<(string Text, int PageNumber)> ExtractTextFromPdf(string filePath, string fileName)
    {
        var results = new List<(string, int)>();
        
        try
        {
            using (var document = PdfDocument.Open(filePath))
            {
                int pageNum = 1;
                foreach (var page in document.GetPages())
                {
                    string text = page.Text;
                    if (!string.IsNullOrWhiteSpace(text))
                    {
                        results.Add((text, pageNum));
                    }
                    pageNum++;
                }
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error extracting text from PDF: {FilePath}", filePath);
        }

        return results;
    }

    /// <summary>
    /// Calculate MD5 hash of a file
    /// </summary>
    private string CalculateFileHash(string filePath)
    {
        using (var md5 = MD5.Create())
        {
            using (var stream = File.OpenRead(filePath))
            {
                var hash = md5.ComputeHash(stream);
                return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
            }
        }
    }

    /// <summary>
    /// Get statistics about processed files
    /// </summary>
    public async Task<(int FileCount, int ChunkCount)> GetStatisticsAsync()
    {
        var fileCount = await _context.Files.CountAsync();
        var chunkCount = await _context.Chunks.CountAsync();
        return (fileCount, chunkCount);
    }
}
