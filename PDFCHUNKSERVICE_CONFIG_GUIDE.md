# PdfChunkService Configuration Guide

## Overview
The PdfChunkService needs to be configured with user-specific paths instead of hardcoded values.

## Required Configuration

The service should ask the user for the following on first run:

1. **PDF Documents Directory**: Where to scan for PDF files
2. **Database Path**: Where to store the database file (should NOT be in project directory)
3. **Database Name**: Name of the database file (e.g., pdfchunks.db)

## Configuration File Structure

The service should use a configuration file stored in a user-accessible location:
- **Location**: `%APPDATA%\PdfSearchAI\service-config.json`
- **Format**: JSON

### Example Configuration File:

```json
{
  "DocumentsDirectory": "C:\\Users\\YourName\\Documents\\PDFs",
  "DatabasePath": "C:\\Users\\YourName\\Documents\\PdfSearchAI\\pdfchunks.db",
  "ScanIntervalDays": 3,
  "EnableOCR": false
}
```

## Implementation Recommendations

### First-Run Configuration

When the service starts for the first time:

1. Check if configuration file exists at `%APPDATA%\PdfSearchAI\service-config.json`
2. If not exists:
   - Show interactive console prompts (if running in console mode)
   - OR create a WPF configuration window (for better UX)
   - Ask for:
     - PDF documents directory (with folder browser)
     - Database location (with file browser)
     - Scan interval (default: 3 days)
3. Save configuration to AppData
4. Validate paths exist/create directories if needed

### Console Mode Example:

```csharp
public static ServiceConfiguration GetOrCreateConfiguration()
{
    var configPath = GetConfigurationPath();
    
    if (!File.Exists(configPath))
    {
        Console.WriteLine("=== PDF Chunk Service - First Run Configuration ===");
        Console.WriteLine();
        
        Console.Write("Enter PDF documents directory path: ");
        var documentsDir = Console.ReadLine();
        
        Console.Write("Enter database file path (e.g., C:\\Data\\pdfchunks.db): ");
        var dbPath = Console.ReadLine();
        
        Console.Write("Scan interval in days (default 3): ");
        var intervalInput = Console.ReadLine();
        var scanInterval = int.TryParse(intervalInput, out var days) ? days : 3;
        
        var config = new ServiceConfiguration
        {
            DocumentsDirectory = documentsDir,
            DatabasePath = dbPath,
            ScanIntervalDays = scanInterval
        };
        
        SaveConfiguration(config);
        return config;
    }
    
    return LoadConfiguration(configPath);
}

private static string GetConfigurationPath()
{
    var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
    var configDir = Path.Combine(appData, "PdfSearchAI");
    Directory.CreateDirectory(configDir);
    return Path.Combine(configDir, "service-config.json");
}
```

## Important Considerations

### Path Validation
- Ensure documents directory exists or can be created
- Ensure database directory exists or can be created
- Check write permissions
- Handle network paths appropriately

### Security
- Store configuration in user AppData (not in application directory)
- Don't expose sensitive paths in logs
- Validate all user inputs

### Default Values
- **Documents Directory**: `%USERPROFILE%\Documents\PDFs`
- **Database Path**: `%USERPROFILE%\Documents\PdfSearchAI\pdfchunks.db`
- **Scan Interval**: 3 days

## Compatibility with InternalAIAssistant

The database path configured in PdfChunkService **MUST match** the path configured in InternalAIAssistant:

- Both should point to the same database file
- InternalAIAssistant reads chunks that PdfChunkService creates
- Consider using a shared configuration or documenting this requirement clearly

## Migration from Hardcoded Paths

If the service currently has hardcoded paths like:
```csharp
var documentsPath = @"C:\Users\admin\Nooshin\docs";
var dbPath = @"C:\Users\admin\Nooshin\pdfchunkservice.db";
```

Replace with:
```csharp
var config = ConfigurationManager.GetOrCreateConfiguration();
var documentsPath = config.DocumentsDirectory;
var dbPath = config.DatabasePath;
```

## Testing

After implementing configuration:
1. Delete existing configuration file
2. Run service
3. Verify configuration dialog/prompts appear
4. Enter test paths
5. Verify configuration is saved
6. Restart service
7. Verify service loads existing configuration without prompting
