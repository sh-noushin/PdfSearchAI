using Microsoft.EntityFrameworkCore;
using PdfChunkService.Configuration;
using PdfChunkService.Data;
using PdfChunkService.Services;
using Serilog;

namespace PdfChunkService;

class Program
{
    static async Task Main(string[] args)
    {
        // Configure Serilog
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.Console()
            .WriteTo.File("logs/pdfchunkservice-.txt", rollingInterval: RollingInterval.Day)
            .CreateLogger();

        try
        {
            Log.Information("=== PDF Chunk Service Starting ===");

            // Get or create configuration
            var config = ConfigurationManager.GetOrCreateConfiguration();
            
            Log.Information("Configuration loaded:");
            Log.Information("  Documents Directory: {DocumentsDirectory}", config.DocumentsDirectory);
            Log.Information("  Database Path: {DatabasePath}", config.DatabasePath);
            Log.Information("  Scan Interval: {ScanInterval} days", config.ScanIntervalDays);

            // Validate paths
            if (!Directory.Exists(config.DocumentsDirectory))
            {
                Log.Warning("Documents directory does not exist, creating: {Directory}", config.DocumentsDirectory);
                Directory.CreateDirectory(config.DocumentsDirectory);
            }

            // Ensure database directory exists
            var dbDirectory = Path.GetDirectoryName(config.DatabasePath);
            if (!string.IsNullOrEmpty(dbDirectory) && !Directory.Exists(dbDirectory))
            {
                Log.Warning("Database directory does not exist, creating: {Directory}", dbDirectory);
                Directory.CreateDirectory(dbDirectory);
            }

            // Determine database provider based on file extension
            var isDbExtension = config.DatabasePath.EndsWith(".db", StringComparison.OrdinalIgnoreCase) ||
                               config.DatabasePath.EndsWith(".sqlite", StringComparison.OrdinalIgnoreCase);
            
            // Setup database context
            var optionsBuilder = new DbContextOptionsBuilder<PdfChunkDbContext>();
            
            if (isDbExtension)
            {
                // Use SQLite
                var connectionString = $"Data Source={config.DatabasePath}";
                optionsBuilder.UseSqlite(connectionString);
                Log.Information("Using SQLite database: {DatabasePath}", config.DatabasePath);
            }
            else
            {
                // Use SQL Server
                optionsBuilder.UseSqlServer(config.DatabasePath);
                Log.Information("Using SQL Server database");
            }

            using (var context = new PdfChunkDbContext(optionsBuilder.Options))
            {
                // Ensure database is created
                Log.Information("Ensuring database exists...");
                await context.Database.EnsureCreatedAsync();
                Log.Information("Database is ready");

                // Create processing service
                var processingService = new PdfProcessingService(context, config.DocumentsDirectory);

                // Process all PDFs
                Log.Information("Starting PDF processing...");
                await processingService.ProcessAllPdfsAsync();

                // Get statistics
                var (fileCount, chunkCount) = await processingService.GetStatisticsAsync();
                Log.Information("Processing complete. Files: {FileCount}, Chunks: {ChunkCount}", fileCount, chunkCount);
            }

            Log.Information("=== PDF Chunk Service Completed Successfully ===");
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Fatal error in PDF Chunk Service");
            Environment.ExitCode = 1;
        }
        finally
        {
            Log.CloseAndFlush();
        }
    }
}
