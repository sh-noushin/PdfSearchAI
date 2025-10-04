using Microsoft.EntityFrameworkCore;
using PdfChunkService.Configuration;
using PdfChunkService.Data;
using PdfChunkService.Services;

namespace PdfChunkService;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly IConfigurationService _configurationService;
    private readonly IPdfProcessingService _pdfProcessingService;
    private readonly IServiceProvider _serviceProvider;
    private ServiceConfiguration? _serviceConfig;

    public Worker(
        ILogger<Worker> logger,
        IConfigurationService configurationService,
        IPdfProcessingService pdfProcessingService,
        IServiceProvider serviceProvider)
    {
        _logger = logger;
        _configurationService = configurationService;
        _pdfProcessingService = pdfProcessingService;
        _serviceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("PDF Chunk Service starting...");

        try
        {
            // Ensure database is created
            using (var scope = _serviceProvider.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<PdfChunkDbContext>();
                await dbContext.Database.EnsureCreatedAsync(stoppingToken);
                _logger.LogInformation("Database initialized successfully");
            }

            // Load or create configuration
            await InitializeConfigurationAsync();

            if (_serviceConfig == null)
            {
                _logger.LogError("Failed to initialize service configuration. Service cannot start.");
                return;
            }

            // Perform initial scan
            if (!string.IsNullOrEmpty(_serviceConfig.DirectoryPath))
            {
                _logger.LogInformation("Performing initial directory scan...");
                await _pdfProcessingService.ProcessDirectoryAsync(_serviceConfig.DirectoryPath, _serviceConfig.ChunkSize);
            }
            else
            {
                _logger.LogError("Service configuration missing DirectoryPath. Skipping initial scan.");
            }

            // Start periodic scanning
            var scanInterval = TimeSpan.FromDays(_serviceConfig.ScanIntervalDays);
            _logger.LogInformation("Starting periodic scanning every {Interval} days", _serviceConfig.ScanIntervalDays);

            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(scanInterval, stoppingToken);

                if (!stoppingToken.IsCancellationRequested && !string.IsNullOrEmpty(_serviceConfig.DirectoryPath))
                {
                    _logger.LogInformation("Performing scheduled directory scan...");
                    await _pdfProcessingService.ProcessDirectoryAsync(_serviceConfig.DirectoryPath, _serviceConfig.ChunkSize);
                }
                else if (_serviceConfig != null && string.IsNullOrEmpty(_serviceConfig.DirectoryPath))
                {
                    _logger.LogError("Service configuration missing DirectoryPath. Skipping scheduled scan.");
                }
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("PDF Chunk Service stopping due to cancellation");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "PDF Chunk Service encountered an error");
            throw;
        }

        _logger.LogInformation("PDF Chunk Service stopped");
    }

    private async Task InitializeConfigurationAsync()
    {
        try
        {
            _serviceConfig = await _configurationService.LoadConfigurationAsync();
            if (_serviceConfig == null)
            {
                _logger.LogError("Failed to obtain configuration interactively. Service cannot continue.");
                return;
            }
            _logger.LogInformation("Service configuration loaded - Directory: {DirectoryPath}, DB: {DB}, Scan Interval: {ScanInterval} days", 
                _serviceConfig.DirectoryPath, _serviceConfig.ConnectionString, _serviceConfig.ScanIntervalDays);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to initialize configuration");
        }
    }
}
