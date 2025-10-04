using Microsoft.Extensions.Logging;
using Moq;
using PdfChunkService.Configuration;
using PdfChunkService.Services;

namespace PdfChunkService.Tests;

public class ConfigurationServiceTests
{
    private readonly Mock<ILogger<ConfigurationService>> _mockLogger;
    private readonly ConfigurationService _configurationService;
    private readonly string _testConfigPath;

    public ConfigurationServiceTests()
    {
        _mockLogger = new Mock<ILogger<ConfigurationService>>();
        _configurationService = new ConfigurationService();
        _testConfigPath = Path.Combine(Path.GetTempPath(), "test-service-config.json");
    }

    [Fact]
    public async Task LoadConfigurationAsync_WhenFileDoesNotExist_ReturnsNull()
    {
        // Arrange - ensure config file doesn't exist
        var configFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "service-config.json");
        if (File.Exists(configFilePath))
        {
            File.Delete(configFilePath);
        }
        
        // Act
        var result = await _configurationService.LoadConfigurationAsync();
        
        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task SaveAndLoadConfigurationAsync_ValidConfiguration_WorksCorrectly()
    {
        // Arrange
        var config = new ServiceConfiguration
        {
            DirectoryPath = @"C:\TestPath",
            ScanIntervalDays = 5,
            ChunkSize = 2000,
            ConnectionString = "test connection string"
        };

        // Act
        await _configurationService.SaveConfigurationAsync(config);
        var loadedConfig = await _configurationService.LoadConfigurationAsync();
        
        // Assert
        Assert.NotNull(loadedConfig);
        Assert.Equal(config.DirectoryPath, loadedConfig.DirectoryPath);
        Assert.Equal(config.ScanIntervalDays, loadedConfig.ScanIntervalDays);
        Assert.Equal(config.ChunkSize, loadedConfig.ChunkSize);
        Assert.Equal(config.ConnectionString, loadedConfig.ConnectionString);
        
        // Cleanup - delete the configuration file
        var configFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "service-config.json");
        if (File.Exists(configFilePath))
        {
            File.Delete(configFilePath);
        }
    }

    [Fact]
    public async Task PromptForDirectoryPathAsync_WithEnvironmentVariable_ReturnsEnvironmentPath()
    {
        // Arrange
        var envPath = Path.GetTempPath();
        Environment.SetEnvironmentVariable("PDF_DIRECTORY_PATH", envPath);

        try
        {
            // Act
            var result = await _configurationService.PromptForDirectoryPathAsync();

            // Assert
            Assert.Equal(envPath, result);
        }
        finally
        {
            // Cleanup
            Environment.SetEnvironmentVariable("PDF_DIRECTORY_PATH", null);
        }
    }

    [Fact]
    public async Task PromptForDirectoryPathAsync_WithoutEnvironmentVariable_ReturnsDefaultPath()
    {
        // Arrange
        Environment.SetEnvironmentVariable("PDF_DIRECTORY_PATH", null);

        // Act
        var result = await _configurationService.PromptForDirectoryPathAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Contains("PDFFiles", result);
    }
}