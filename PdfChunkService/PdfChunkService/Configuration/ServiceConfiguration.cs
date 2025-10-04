namespace PdfChunkService.Configuration;

public class ServiceConfiguration
{
    public string DirectoryPath { get; set; } = string.Empty;
    public int ScanIntervalDays { get; set; } = 3;
    public string ConnectionString { get; set; } = string.Empty;
    public int ChunkSize { get; set; } = 1000; // Characters per chunk
}