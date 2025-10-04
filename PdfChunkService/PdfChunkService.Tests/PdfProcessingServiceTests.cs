using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using PdfChunkService.Data;
using PdfChunkService.Services;

namespace PdfChunkService.Tests;

public class PdfProcessingServiceTests : IDisposable
{
    private readonly Mock<ILogger<PdfProcessingService>> _mockLogger;
    private readonly PdfChunkDbContext _dbContext;
    private readonly PdfProcessingService _pdfProcessingService;

    public PdfProcessingServiceTests()
    {
        _mockLogger = new Mock<ILogger<PdfProcessingService>>();

        var options = new DbContextOptionsBuilder<PdfChunkDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _dbContext = new PdfChunkDbContext(options);

        var services = new ServiceCollection();
        services.AddSingleton<PdfChunkDbContext>(_dbContext);
        var serviceProvider = services.BuildServiceProvider();

        _pdfProcessingService = new PdfProcessingService(serviceProvider, _mockLogger.Object);
    }

    [Fact]
    public async Task ProcessDirectoryAsync_WithNonExistentDirectory_ReturnsFalse()
    {
        // Arrange
        var nonExistentPath = Path.Combine(Path.GetTempPath(), "non-existent-directory");

        // Act
        var result = await _pdfProcessingService.ProcessDirectoryAsync(nonExistentPath, 1000);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task ProcessDirectoryAsync_WithEmptyDirectory_ReturnsTrue()
    {
        // Arrange
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);

        try
        {
            // Act
            var result = await _pdfProcessingService.ProcessDirectoryAsync(tempDir, 1000);

            // Assert
            Assert.True(result);
        }
        finally
        {
            // Cleanup
            if (Directory.Exists(tempDir))
                Directory.Delete(tempDir, true);
        }
    }

    [Fact]
    public async Task ProcessFileAsync_WithNonExistentFile_ReturnsFalse()
    {
        // Arrange
        var nonExistentFile = Path.Combine(Path.GetTempPath(), "non-existent.pdf");

        // Act
        var result = await _pdfProcessingService.ProcessFileAsync(nonExistentFile, 1000);

        // Assert
        Assert.False(result);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
    }
}