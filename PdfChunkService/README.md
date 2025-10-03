# PdfChunkService

A Windows service for automated PDF processing and database storage for the PdfSearchAI system.

## Overview

PdfChunkService is a background service that:
- Monitors a specified directory for PDF files
- Extracts text content from PDFs using PdfPig
- Splits content into searchable chunks
- Stores chunks in a SQLite database
- Automatically detects file changes using MD5 hashing
- Periodically rescans for new or updated files

## First-Run Configuration

When you run the service for the first time, it will prompt you for configuration:

```
=== PDF Chunk Service - First Run Configuration ===

Enter the directory to scan for PDF files:
(Default: C:\Users\YourName\Documents\PDFs)
> 

Enter the database file path:
(Default: C:\Users\YourName\Documents\PdfSearchAI\pdfchunks.db)
> 

Scan interval in days (default 3): 
> 
```

### Configuration Details

1. **Documents Directory**: The folder where your PDF files are stored
   - Can be any accessible path on your system
   - The service will scan this directory and all subdirectories
   - Files are monitored for changes

2. **Database Path**: Location where the SQLite database will be created
   - Should be the same path used by InternalAIAssistant
   - Both applications must point to the same database file
   - Default location: `Documents\PdfSearchAI\pdfchunks.db`

3. **Scan Interval**: How often to check for new/updated files (in days)
   - Default: 3 days
   - Files are immediately processed on first run

## Configuration File

Configuration is saved at:
```
%APPDATA%\PdfSearchAI\service-config.json
```

Example configuration:
```json
{
  "DocumentsDirectory": "C:\\Users\\YourName\\Documents\\PDFs",
  "DatabasePath": "C:\\Users\\YourName\\Documents\\PdfSearchAI\\pdfchunks.db",
  "ScanIntervalDays": 3,
  "EnableOCR": false
}
```

## Running the Service

### Development/Console Mode

For testing and development, run as a console application:

```bash
dotnet run --project PdfChunkService
```

The service will:
1. Prompt for configuration (if first run)
2. Create the database
3. Begin scanning and processing PDFs
4. Log activity to the console

### Production/Windows Service Mode

To install as a Windows service:

1. Publish the application:
   ```bash
   dotnet publish -c Release -o C:\PdfChunkService
   ```

2. Install the service (requires Administrator):
   ```bash
   sc create "PDF Chunk Service" binPath="C:\PdfChunkService\PdfChunkService.exe"
   ```

3. Start the service:
   ```bash
   sc start "PDF Chunk Service"
   ```

## Database Coordination with InternalAIAssistant

**IMPORTANT**: Both PdfChunkService and InternalAIAssistant must use the same database file.

### Setup Process

1. **Install PdfChunkService First**
   - Run the service
   - Configure PDF directory and database path
   - Let it process your PDF files

2. **Install InternalAIAssistant**
   - Run the application
   - When prompted, select the **same database path** as PdfChunkService
   - The application will read chunks created by the service

3. **Verify Paths Match**
   ```
   PdfChunkService:      C:\Users\...\pdfchunks.db
   InternalAIAssistant:  C:\Users\...\pdfchunks.db
                          ^ Must be identical ^
   ```

## How It Works

1. **File Discovery**: Scans the configured directory for `.pdf` files
2. **Change Detection**: Uses MD5 hash to detect if files have changed
3. **Text Extraction**: Uses PdfPig to extract text from each page
4. **Chunking**: Splits text into ~500 character chunks for efficient searching
5. **Database Storage**: Stores files and chunks in SQLite with proper relationships
6. **Periodic Scanning**: Automatically rescans at configured intervals

## Database Schema

### Files Table
- `Id`: Primary key
- `FileName`: Name of the PDF file
- `FilePath`: Full path to the file
- `FileHash`: MD5 hash for change detection
- `FileSize`: Size in bytes
- `CreatedAt`: When first processed
- `LastModified`: File's last modification time

### Chunks Table
- `Id`: Primary key
- `FileId`: Foreign key to Files table
- `Content`: The text chunk
- `ChunkIndex`: Order within the file
- `PageNumber`: Which page the chunk came from
- `CreatedAt`: When the chunk was created

## Logging

The service logs to:
- Console (when running in console mode)
- Windows Event Log (when running as a service)

Log levels:
- **Information**: Normal operations (file processing, startup)
- **Warning**: Missing directories or configuration issues
- **Error**: Processing failures, exceptions

## Troubleshooting

### Configuration Not Found
If configuration is missing or invalid, the service will prompt for new configuration on startup.

### Files Not Being Processed
1. Check that the documents directory exists and contains PDF files
2. Verify the service has read permissions for the directory
3. Check logs for error messages
4. Ensure PDFs are not corrupted

### Database Issues
1. Verify the database directory exists
2. Check write permissions for the database path
3. Ensure InternalAIAssistant is not locking the database

### Restarting Configuration
To reconfigure the service:
1. Stop the service
2. Delete: `%APPDATA%\PdfSearchAI\service-config.json`
3. Restart the service (it will prompt for new configuration)

## Requirements

- .NET 8.0 or later
- Windows operating system
- Read access to PDF directory
- Write access to database location
- SQLite support

## Dependencies

- Microsoft.EntityFrameworkCore
- Microsoft.EntityFrameworkCore.Sqlite
- PdfPig (for PDF text extraction)
- Microsoft.Extensions.Hosting (for Windows service support)
