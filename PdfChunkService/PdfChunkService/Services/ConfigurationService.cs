using System.Text.Json;
using PdfChunkService.Configuration;

namespace PdfChunkService.Services;

public interface IConfigurationService
{
    Task<ServiceConfiguration?> LoadConfigurationAsync();
    Task SaveConfigurationAsync(ServiceConfiguration configuration);
    Task<string?> PromptForDirectoryPathAsync();
}

public class ConfigurationService : IConfigurationService
{

    public ConfigurationService()
    {
        // No config file logic needed
    }

    public Task<ServiceConfiguration?> LoadConfigurationAsync()
    {
        // Always prompt interactively
        Console.WriteLine("PDF Chunk Service Setup:");
        Console.WriteLine("Enter the full path to your PDF directory:");
        string? pdfPath = Console.ReadLine();
        if (string.IsNullOrWhiteSpace(pdfPath) || !Directory.Exists(pdfPath))
        {
            Console.WriteLine("Invalid PDF directory. Exiting.");
            return Task.FromResult<ServiceConfiguration?>(null);
        }
        Console.WriteLine("Enter the full path and filename for your database (e.g. C:\\MyData\\mydb.db):");
        string? dbPath = Console.ReadLine();
        if (string.IsNullOrWhiteSpace(dbPath))
        {
            Console.WriteLine("Invalid database path. Exiting.");
            return Task.FromResult<ServiceConfiguration?>(null);
        }
        Console.WriteLine("How many days between scans? (default 3):");
        string? scanDaysStr = Console.ReadLine();
        int scanDays = 3;
        int.TryParse(scanDaysStr, out scanDays);
        Console.WriteLine("How many characters per chunk? (default 1000):");
        string? chunkSizeStr = Console.ReadLine();
        int chunkSize = 1000;
        int.TryParse(chunkSizeStr, out chunkSize);

        var config = new ServiceConfiguration
        {
            DirectoryPath = pdfPath,
            ScanIntervalDays = scanDays,
            ChunkSize = chunkSize,
            ConnectionString = $"Data Source={dbPath}"
        };
        return Task.FromResult<ServiceConfiguration?>(config);
    }

    public Task SaveConfigurationAsync(ServiceConfiguration configuration)
    {
        // No-op
        return Task.CompletedTask;
    }

    public Task<string?> PromptForDirectoryPathAsync()
    {
        // Interactive prompt for PDF directory and DB path
        if (Environment.UserInteractive)
        {
            Console.WriteLine("Enter the full path to your PDF directory:");
            string? pdfPath = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(pdfPath) || !Directory.Exists(pdfPath))
            {
                Console.WriteLine("Invalid PDF directory. Exiting.");
                return Task.FromResult<string?>(null);
            }
            Console.WriteLine("Enter the full path and filename for your database (e.g. C:\\MyData\\mydb.db):");
            string? dbPath = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(dbPath))
            {
                Console.WriteLine("Invalid database path. Exiting.");
                return Task.FromResult<string?>(null);
            }
            // Save to ServiceConfiguration
            var config = new ServiceConfiguration
            {
                DirectoryPath = pdfPath,
                ConnectionString = $"Data Source={dbPath}",
                ScanIntervalDays = 3,
                ChunkSize = 1000
            };
            SaveConfigurationAsync(config).Wait();
            return Task.FromResult<string?>(pdfPath);
        }
        // Fallback: use environment variable if set
        var envPath = Environment.GetEnvironmentVariable("PDF_DIRECTORY_PATH");
        if (!string.IsNullOrEmpty(envPath) && Directory.Exists(envPath))
        {
            Console.WriteLine($"Using directory path from environment variable: {envPath}");
            return Task.FromResult<string?>(envPath);
        }
        // No valid path found
        Console.WriteLine("No valid PDF directory found. Please run interactively to set up paths.");
        return Task.FromResult<string?>(null);
    }
}