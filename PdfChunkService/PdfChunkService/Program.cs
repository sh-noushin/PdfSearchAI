using PdfChunkService.Configuration;
using Microsoft.EntityFrameworkCore;
using PdfChunkService;
using PdfChunkService.Data;
using PdfChunkService.Services;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.File("logs/pdf-chunk-service-.txt", rollingInterval: RollingInterval.Day)
    .WriteTo.Console()
    .CreateLogger();

try
{
    Console.WriteLine("PDF Chunk Service Setup:");
    Console.WriteLine("Enter the full path to your PDF directory:");
    string? pdfPath = Console.ReadLine();
    if (string.IsNullOrWhiteSpace(pdfPath))
    {
        Console.WriteLine("Invalid PDF directory. Exiting.");
        return;
    }
    if (!Directory.Exists(pdfPath))
    {
        Directory.CreateDirectory(pdfPath);
        Console.WriteLine($"Created PDF directory: {pdfPath}");
    }
    Console.WriteLine("Enter the full path and filename for your database (e.g. C:\\MyData\\mydb.db):");
    string? dbPath = Console.ReadLine();
    if (string.IsNullOrWhiteSpace(dbPath))
    {
        Console.WriteLine("Invalid database path. Exiting.");
        return;
    }
    Console.WriteLine("How many days between scans? (default 3):");
    string? scanDaysStr = Console.ReadLine();
    int scanDays = 3;
    int.TryParse(scanDaysStr, out scanDays);
    Console.WriteLine("How many characters per chunk? (default 1000):");
    string? chunkSizeStr = Console.ReadLine();
    int chunkSize = 1000;
    int.TryParse(chunkSizeStr, out chunkSize);

    // Ensure DB folder exists
    var dbDir = Path.GetDirectoryName(dbPath);
    if (!string.IsNullOrEmpty(dbDir) && !Directory.Exists(dbDir))
    {
        Directory.CreateDirectory(dbDir);
        Console.WriteLine($"Created database folder: {dbDir}");
    }

    var config = new ServiceConfiguration
    {
        DirectoryPath = pdfPath,
        ScanIntervalDays = scanDays,
        ChunkSize = chunkSize,
        ConnectionString = $"Data Source={dbPath}"
    };

    var builder = Host.CreateApplicationBuilder(args);

    // Add Serilog
    builder.Services.AddSerilog();

    builder.Services.AddSingleton(config);
    builder.Services.AddDbContext<PdfChunkDbContext>(options =>
    {
        options.UseSqlite(config.ConnectionString);
    });

    // Register DDD services
    builder.Services.AddSingleton<IConfigurationService, ConfigurationService>();
    builder.Services.AddSingleton<IPdfProcessingService, PdfProcessingService>();

    // Register Worker
    builder.Services.AddHostedService<Worker>();

    // Register as Windows Service (for .NET 8+)
    builder.Services.AddWindowsService(options =>
    {
        options.ServiceName = "PDF Chunk Service";
    });

    var host = builder.Build();

    // Apply EF Core Migrations
    try
    {
        using (var scope = host.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<PdfChunkDbContext>();
            db.Database.Migrate();
        }
    }
    catch (Exception dbEx)
    {
        Log.Error(dbEx, "Database migration failed");
        throw;
    }

    Log.Information("Starting PDF Chunk Service");
    await host.RunAsync();
}
catch (Exception ex)
{
    Log.Fatal(ex, "PDF Chunk Service terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}