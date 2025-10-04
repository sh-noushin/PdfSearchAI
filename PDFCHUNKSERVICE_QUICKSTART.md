# PdfChunkService Quick Start Guide

This guide will help you get started with the PdfChunkService application.

## What is PdfChunkService?

PdfChunkService is a console application that:
1. Scans a directory for PDF files
2. Extracts text from each page
3. Stores the text chunks in a database
4. Tracks file changes to avoid reprocessing

The database created by PdfChunkService is then used by InternalAIAssistant to provide AI-powered search and chat capabilities.

## Prerequisites

- .NET 8.0 SDK
- Windows operating system
- PDF files to process

## Quick Start

### Step 1: Build the Application

```bash
cd PdfChunkService
dotnet build
```

### Step 2: Run the Application

```bash
dotnet run
```

On first run, you'll be prompted to configure:

1. **PDF Documents Directory**: Where your PDF files are located
   - Example: `C:\Users\YourName\Documents\PDFs`
   - The service will scan this directory recursively

2. **Database Path**: Where to store the SQLite database
   - Example: `C:\Users\YourName\Documents\PdfSearchAI\pdfchunks.db`
   - This must be the same path used by InternalAIAssistant

3. **Scan Interval**: How often to scan (currently not used, just press Enter)

### Step 3: Verify Processing

The application will:
- Create the database directory if it doesn't exist
- Scan for PDF files
- Process each PDF and extract text
- Store chunks in the database
- Display statistics (e.g., "Files: 3, Chunks: 9")

### Step 4: Check the Results

You can verify the database was created and populated:

```bash
# On Linux/Mac
sqlite3 /path/to/pdfchunks.db "SELECT COUNT(*) FROM Files;"
sqlite3 /path/to/pdfchunks.db "SELECT COUNT(*) FROM Chunks;"

# On Windows (if SQLite is installed)
sqlite3.exe C:\path\to\pdfchunks.db "SELECT COUNT(*) FROM Files;"
sqlite3.exe C:\path\to\pdfchunks.db "SELECT COUNT(*) FROM Chunks;"
```

## Running Again

On subsequent runs, the application will:
1. Load the saved configuration
2. Skip unchanged files (using MD5 hash comparison)
3. Only process new or modified files

## Configuration File Location

Configuration is stored at:
- **Windows**: `%APPDATA%\PdfSearchAI\service-config.json`
- **Linux/Mac**: `~/.config/PdfSearchAI/service-config.json`

To reconfigure:
1. Delete the configuration file
2. Run the application again

## Troubleshooting

### No chunks created

**Possible causes:**
- No PDF files in the specified directory
- PDFs are scanned images (no text to extract)
- Permission issues reading files

**Solution:** Check the logs in the `logs/` directory for detailed error messages.

### Database is empty

**Check:**
1. Verify PDF files exist in the configured directory
2. Check if PDFs contain actual text (not just images)
3. Review the logs for processing errors

### "Database directory does not exist" error

**Solution:** The application should create the directory automatically. If it fails, create the directory manually and ensure you have write permissions.

## Integration with InternalAIAssistant

After running PdfChunkService:

1. Note the database path you configured
2. Run InternalAIAssistant
3. When prompted, provide the **same database path**
4. InternalAIAssistant will read the chunks and enable AI search

## Logs

Logs are stored in the `logs/` directory with daily rotation:
- `logs/pdfchunkservice-YYYYMMDD.txt`

The logs include:
- Configuration details
- Files processed
- Errors and warnings
- Statistics

## Example Workflow

```bash
# 1. Create a directory with PDFs
mkdir ~/Documents/PDFs
cp some-documents.pdf ~/Documents/PDFs/

# 2. Run PdfChunkService
cd PdfChunkService
dotnet run

# When prompted:
# - Documents Directory: /home/user/Documents/PDFs
# - Database Path: /home/user/Documents/PdfSearchAI/pdfchunks.db
# - Scan Interval: (press Enter)

# 3. Verify results
# Output should show: "Files: X, Chunks: Y"

# 4. Run InternalAIAssistant with the same database path
cd ../InternalAIAssistant
dotnet run
```

## Common Commands

### Clean rebuild
```bash
dotnet clean
dotnet build
```

### Run without rebuilding
```bash
dotnet run --no-build
```

### Release build
```bash
dotnet build --configuration Release
cd bin/Release/net8.0-windows
./PdfChunkService.exe
```

## Next Steps

After successfully running PdfChunkService:

1. ✅ PDFs are processed and chunks are in the database
2. ✅ Configuration is saved for future runs
3. ➡️ Run InternalAIAssistant to search and chat with your documents
4. ➡️ Add more PDFs and run PdfChunkService again to update the database

## Need Help?

- Check the `README.md` in the PdfChunkService directory for detailed documentation
- Review logs in the `logs/` directory
- Ensure both applications use the same database path
