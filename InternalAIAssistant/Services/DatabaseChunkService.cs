using Microsoft.EntityFrameworkCore;
using InternalAIAssistant.Data;
using InternalAIAssistant.Models;

namespace InternalAIAssistant.Services;

public class DatabaseChunkService
{
    private readonly PdfChunkDbContext _context;

    public DatabaseChunkService(PdfChunkDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Get all chunks from the database and convert them to DocumentChunk format
    /// </summary>
    public async Task<List<DocumentChunk>> GetAllChunksAsync()
    {
        var chunks = await _context.Chunks
            .Include(c => c.File)
            .Select(c => new DocumentChunk
            {
                Text = c.Content,
                FileName = c.File.FileName,
                Page = c.PageNumber,
                Embedding = new float[0] // Will be generated on demand if needed
            })
            .ToListAsync();

        return chunks;
    }

    /// <summary>
    /// Get chunks for a specific file
    /// </summary>
    public async Task<List<DocumentChunk>> GetChunksByFileAsync(string fileName)
    {
        var chunks = await _context.Chunks
            .Include(c => c.File)
            .Where(c => c.File.FileName == fileName)
            .Select(c => new DocumentChunk
            {
                Text = c.Content,
                FileName = c.File.FileName,
                Page = c.PageNumber,
                Embedding = new float[0]
            })
            .ToListAsync();

        return chunks;
    }

    /// <summary>
    /// Get file statistics
    /// </summary>
    public async Task<(int FileCount, int ChunkCount)> GetStatisticsAsync()
    {
        var fileCount = await _context.Files.CountAsync();
        var chunkCount = await _context.Chunks.CountAsync();
        return (fileCount, chunkCount);
    }

    /// <summary>
    /// Get recently processed files (within the last N days)
    /// </summary>
    public async Task<List<string>> GetRecentFilesAsync(int days = 7)
    {
        var cutoffDate = DateTime.Now.AddDays(-days);
        var recentFiles = await _context.Files
            .Where(f => f.CreatedAt >= cutoffDate)
            .Select(f => f.FileName)
            .ToListAsync();

        return recentFiles;
    }
}