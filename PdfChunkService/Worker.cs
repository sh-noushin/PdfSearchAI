using PdfChunkService.Configuration;
using PdfChunkService.Data;
using PdfChunkService.Models;
using UglyToad.PdfPig;
using System.Security.Cryptography;
using System.Text;

namespace PdfChunkService;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly ServiceConfiguration _config;
    private readonly IServiceProvider _serviceProvider;

    public Worker(ILogger<Worker> logger, ServiceConfiguration config, IServiceProvider serviceProvider)
    {
        _logger = logger;
        _config = config;
        _serviceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("PDF Chunk Service started");
        _logger.LogInformation($"Scanning directory: {_config.DocumentsDirectory}");
        _logger.LogInformation($"Database location: {_config.DatabasePath}");
        _logger.LogInformation($"Scan interval: {_config.ScanIntervalDays} days");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessPdfFilesAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing PDF files");
            }

            // Wait for the configured interval
            await Task.Delay(TimeSpan.FromDays(_config.ScanIntervalDays), stoppingToken);
        }
    }

    private async Task ProcessPdfFilesAsync(CancellationToken stoppingToken)
    {
        if (!Directory.Exists(_config.DocumentsDirectory))
        {
            _logger.LogWarning($"Documents directory does not exist: {_config.DocumentsDirectory}");
            return;
        }

        var pdfFiles = Directory.GetFiles(_config.DocumentsDirectory, "*.pdf", SearchOption.AllDirectories);
        _logger.LogInformation($"Found {pdfFiles.Length} PDF files");

        using var scope = _serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<PdfChunkDbContext>();

        foreach (var pdfFile in pdfFiles)
        {
            if (stoppingToken.IsCancellationRequested)
                break;

            try
            {
                await ProcessSinglePdfAsync(pdfFile, dbContext, stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error processing file: {pdfFile}");
            }
        }

        await dbContext.SaveChangesAsync(stoppingToken);
    }

    private async Task ProcessSinglePdfAsync(string filePath, PdfChunkDbContext dbContext, CancellationToken stoppingToken)
    {
        var fileInfo = new FileInfo(filePath);
        var fileName = Path.GetFileName(filePath);
        var fileHash = ComputeFileHash(filePath);

        // Check if file already processed
        var existingFile = await dbContext.Files
            .FirstOrDefaultAsync(f => f.FilePath == filePath, stoppingToken);

        if (existingFile != null && existingFile.FileHash == fileHash)
        {
            _logger.LogDebug($"File already processed: {fileName}");
            return;
        }

        _logger.LogInformation($"Processing: {fileName}");

        // Remove old chunks if file was updated
        if (existingFile != null)
        {
            dbContext.Files.Remove(existingFile);
            await dbContext.SaveChangesAsync(stoppingToken);
        }

        // Create new file entity
        var fileEntity = new FileEntity
        {
            FileName = fileName,
            FilePath = filePath,
            FileHash = fileHash,
            FileSize = fileInfo.Length,
            CreatedAt = DateTime.Now,
            LastModified = fileInfo.LastWriteTime
        };

        dbContext.Files.Add(fileEntity);
        await dbContext.SaveChangesAsync(stoppingToken);

        // Extract text and create chunks
        using (var document = PdfDocument.Open(filePath))
        {
            int chunkIndex = 0;

            foreach (var page in document.GetPages())
            {
                if (stoppingToken.IsCancellationRequested)
                    break;

                var text = page.Text;
                if (string.IsNullOrWhiteSpace(text))
                    continue;

                // Split into chunks (roughly 500 characters each)
                var chunks = SplitIntoChunks(text, 500);

                foreach (var chunkText in chunks)
                {
                    var chunk = new ChunkEntity
                    {
                        FileId = fileEntity.Id,
                        Content = chunkText,
                        ChunkIndex = chunkIndex++,
                        PageNumber = page.Number,
                        CreatedAt = DateTime.Now
                    };

                    dbContext.Chunks.Add(chunk);
                }
            }
        }

        await dbContext.SaveChangesAsync(stoppingToken);
        _logger.LogInformation($"Successfully processed: {fileName} ({chunkIndex} chunks)");
    }

    private string ComputeFileHash(string filePath)
    {
        using var md5 = MD5.Create();
        using var stream = File.OpenRead(filePath);
        var hash = md5.ComputeHash(stream);
        return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
    }

    private List<string> SplitIntoChunks(string text, int chunkSize)
    {
        var chunks = new List<string>();
        var words = text.Split(new[] { ' ', '\n', '\r', '\t' }, StringSplitOptions.RemoveEmptyEntries);
        var currentChunk = new StringBuilder();

        foreach (var word in words)
        {
            if (currentChunk.Length + word.Length + 1 > chunkSize && currentChunk.Length > 0)
            {
                chunks.Add(currentChunk.ToString().Trim());
                currentChunk.Clear();
            }

            currentChunk.Append(word);
            currentChunk.Append(' ');
        }

        if (currentChunk.Length > 0)
        {
            chunks.Add(currentChunk.ToString().Trim());
        }

        return chunks;
    }
}
