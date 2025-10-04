# PdfChunkService

A Windows console application that automatically processes PDF documents, extracts text content, creates chunks, and stores them in a database for use by the InternalAIAssistant application.

## Overview

PdfChunkService is responsible for:
- Monitoring a configured directory for PDF files
- Extracting text content from PDFs
- Creating text chunks (one per page)
- Storing file metadata and chunks in a SQLite/SQL Server database
- Tracking file changes using MD5 hashing to avoid reprocessing unchanged files

## Features

- ✅ **Interactive Configuration**: First-run wizard asks for PDF directory and database path
- ✅ **Persistent Configuration**: Settings stored in `%APPDATA%\PdfSearchAI\service-config.json`
- ✅ **Automatic Directory Creation**: Creates directories if they don't exist
- ✅ **Change Detection**: Uses MD5 hashing and file modification timestamps
- ✅ **SQLite Support**: Works with `.db` and `.sqlite` file extensions
- ✅ **SQL Server Support**: Works with SQL Server connection strings
- ✅ **Comprehensive Logging**: Detailed logs stored in `logs/` directory with daily rotation
- ✅ **Error Handling**: Graceful error handling with detailed error messages

## First-Time Setup

When you run the service for the first time, it will prompt you for:

1. **PDF Documents Directory**: Where to scan for PDF files
   - Default: `%USERPROFILE%\Documents\PDFs`
   - Example: `C:\Users\YourName\Documents\PDFs`

2. **Database Path**: Where to store the database file
   - Default: `%USERPROFILE%\Documents\PdfSearchAI\pdfchunks.db`
   - Example: `C:\Users\YourName\Documents\PdfSearchAI\pdfchunks.db`

3. **Scan Interval**: How often to scan for new files (in days)
   - Default: 3 days

The configuration is saved automatically and will be used for subsequent runs.

## Running the Service

### Console Mode (Development/Testing)

```bash
cd PdfChunkService
dotnet run
```

This will:
1. Load or create configuration
2. Create database if it doesn't exist
3. Scan the configured directory for PDFs
4. Process all PDFs and create chunks
5. Display statistics and exit

### Compiled Executable

```bash
cd PdfChunkService
dotnet build --configuration Release
cd bin/Release/net8.0-windows
./PdfChunkService.exe
```

## Database Schema

The service creates two tables:

### Files Table
- `Id` (int): Primary key
- `FileName` (string): Name of the PDF file
- `FilePath` (string): Full path to the PDF file (unique)
- `FileHash` (string): MD5 hash of the file content
- `FileSize` (long): Size in bytes
- `CreatedAt` (DateTime): When first processed
- `LastModified` (DateTime): File's last modification time

### Chunks Table
- `Id` (int): Primary key
- `FileId` (int): Foreign key to Files table
- `Content` (string): Extracted text content
- `ChunkIndex` (int): Sequential index of the chunk
- `PageNumber` (int): Page number from the PDF
- `CreatedAt` (DateTime): When the chunk was created

## Configuration File

The configuration is stored at: `%APPDATA%\PdfSearchAI\service-config.json`

Example:
```json
{
  "DocumentsDirectory": "C:\\Users\\YourName\\Documents\\PDFs",
  "DatabasePath": "C:\\Users\\YourName\\Documents\\PdfSearchAI\\pdfchunks.db",
  "ScanIntervalDays": 3,
  "EnableOCR": false
}
```

To reconfigure the service:
1. Delete the configuration file
2. Run the service again to go through the setup wizard

## Integration with InternalAIAssistant

**Important**: Both PdfChunkService and InternalAIAssistant must use the **same database file**.

1. Configure PdfChunkService first (creates the database)
2. Note the database path you configured
3. Configure InternalAIAssistant to use the same database path
4. Run PdfChunkService to populate the database
5. Run InternalAIAssistant to query the chunks

## Logging

Logs are stored in the `logs/` directory with daily rotation:
- `logs/pdfchunkservice-YYYYMMDD.txt`

Log levels:
- **Information**: Normal operations and progress
- **Warning**: Non-critical issues (e.g., file already processed)
- **Error**: Processing errors for specific files
- **Fatal**: Critical errors that prevent service execution

## How It Works

1. **Startup**
   - Loads configuration from AppData
   - Connects to database (SQLite or SQL Server)
   - Ensures database schema exists

2. **PDF Discovery**
   - Scans configured directory recursively
   - Finds all `.pdf` files

3. **File Processing**
   - For each PDF file:
     - Calculate MD5 hash
     - Check if file exists in database
     - If exists and unchanged: Skip
     - If new or changed: Process
       - Extract text from each page
       - Create chunk for each page
       - Store in database

4. **Statistics**
   - Display total files processed
   - Display total chunks created

## Troubleshooting

### "Database directory does not exist"
**Solution**: The service automatically creates missing directories. Check write permissions.

### "Error processing PDF file"
**Solution**: 
- Check if the PDF is corrupted
- Check if the PDF is password-protected
- Check logs for detailed error message

### "Configuration saved successfully" but database is empty
**Solution**: 
- Ensure PDF files exist in the configured directory
- Check if PDFs contain text (not scanned images)
- Review logs for processing errors

### Files are not being processed
**Solution**:
- Check that the documents directory contains `.pdf` files
- Verify the directory path is correct
- Check file permissions
- Review logs for errors

## Development

### Project Structure
```
PdfChunkService/
├── Configuration/
│   └── ConfigurationManager.cs   # Configuration management
├── Data/
│   └── PdfChunkDbContext.cs      # Entity Framework context
├── Models/
│   ├── FileEntity.cs             # File entity model
│   └── ChunkEntity.cs            # Chunk entity model
├── Services/
│   └── PdfProcessingService.cs   # PDF processing logic
├── Program.cs                     # Main entry point
└── PdfChunkService.csproj        # Project file
```

### Dependencies
- **Microsoft.EntityFrameworkCore.SqlServer**: SQL Server support
- **Microsoft.EntityFrameworkCore.Sqlite**: SQLite support
- **PdfPig**: PDF text extraction
- **Serilog**: Logging framework

### Building
```bash
dotnet restore
dotnet build
```

### Testing
```bash
# Create test directory with sample PDFs
mkdir C:\test-pdfs
# Copy some PDF files to C:\test-pdfs

# Run the service
dotnet run

# When prompted:
# - Documents Directory: C:\test-pdfs
# - Database Path: C:\test-db\test.db
# - Scan Interval: 3 (or press Enter for default)
```

## Future Enhancements

- [ ] Windows Service mode (run as background service)
- [ ] File System Watcher for real-time processing
- [ ] OCR support for scanned PDFs
- [ ] DOCX file support
- [ ] Configurable chunk size (currently one per page)
- [ ] Web API for remote control
- [ ] Dashboard for monitoring

## License

This project is part of the PdfSearchAI solution.
