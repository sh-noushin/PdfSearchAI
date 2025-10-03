# Quick Start Guide - Configuration & Usage

## 🚀 Setup in 3 Steps

### Step 1: Configure PdfChunkService (First Time Only)

Run the service:
```bash
cd PdfChunkService
dotnet run
```

You'll see:
```
=== PDF Chunk Service - First Run Configuration ===

Enter the directory to scan for PDF files:
(Default: C:\Users\YourName\Documents\PDFs)
> [Press Enter for default OR type your path]

Enter the database file path:
(Default: C:\Users\YourName\Documents\PdfSearchAI\pdfchunks.db)
> [Press Enter for default OR type your path]

Scan interval in days (default 3): 
> [Press Enter for default OR type number]
```

**💡 Tip:** Press Enter to accept defaults for quick setup!

---

### Step 2: Configure InternalAIAssistant (First Time Only)

Run the app - a dialog will appear:

```
┌─────────────────────────────────────┐
│  Database Configuration             │
├─────────────────────────────────────┤
│                                     │
│  Database Path:                     │
│  [C:\Users\...\pdfchunks.db] [📁]   │
│                                     │
│  ⚠️ Must match PdfChunkService!     │
│                                     │
│          [Save]    [Cancel]         │
└─────────────────────────────────────┘
```

**💡 Tip:** Click Browse (📁) and select the SAME database path you used in Step 1!

---

### Step 3: Start Using!

That's it! Now:
1. ✅ PdfChunkService processes your PDFs in the background
2. ✅ InternalAIAssistant reads the processed data
3. ✅ Ask questions and get answers with source citations

---

## 📝 Example Usage

### Asking Questions

**You type:**
```
What is the repository pattern?
```

**AI responds:**
```
The repository pattern provides an abstraction layer between 
your data access logic and business logic. It typically 
involves creating an interface that defines common operations 
like Add, Update, Delete, and GetById...

📌 **Source**: design-patterns.pdf, Page 45
```

**Additional sources shown:**
```
📚 **Additional Sources**:
📄 design-patterns.pdf (Pages: 45, 46)
📄 best-practices.pdf (Pages: 12)
```

---

## 🔧 Configuration Files Location

Both apps save their configuration here:
```
Windows: C:\Users\YourName\AppData\Roaming\PdfSearchAI\
├── service-config.json    (PdfChunkService)
└── settings.json          (InternalAIAssistant)
```

---

## ⚠️ Important: Database Path Coordination

**MUST BE THE SAME:**
```
PdfChunkService:     C:\Data\pdfchunks.db  ✅
InternalAIAssistant: C:\Data\pdfchunks.db  ✅
                      ↑ IDENTICAL PATHS ↑
```

**WILL NOT WORK:**
```
PdfChunkService:     C:\Data\pdfchunks.db      ❌
InternalAIAssistant: C:\MyDocs\pdfchunks.db    ❌
                      ↑ DIFFERENT PATHS! ↑
```

---

## 🔄 Reconfiguring

### To Change PdfChunkService Configuration:

1. Stop the service
2. Delete: `%APPDATA%\PdfSearchAI\service-config.json`
3. Restart service (prompts will appear again)

### To Change InternalAIAssistant Configuration:

1. Close the app
2. Delete: `%APPDATA%\PdfSearchAI\settings.json`
3. Restart app (dialog will appear again)

---

## 🐛 Troubleshooting

### Problem: "No documents found" in AI responses

**Check:**
- ✅ Did PdfChunkService process your PDFs? Check the console output
- ✅ Are the database paths the same in both apps?
- ✅ Does the PDF directory actually contain PDF files?

### Problem: Configuration dialog doesn't appear

**Fix:**
- Delete the configuration file (see "Reconfiguring" above)
- Restart the application

### Problem: PDFs not being processed

**Check:**
- ✅ Is the directory path correct?
- ✅ Do you have read permissions for the PDFs?
- ✅ Are they valid PDF files (not corrupted)?
- ✅ Check PdfChunkService console for error messages

---

## 📚 Default Paths

If you accept all defaults:

```
PDF Documents:  C:\Users\YourName\Documents\PDFs\
Database:       C:\Users\YourName\Documents\PdfSearchAI\pdfchunks.db
Config Files:   C:\Users\YourName\AppData\Roaming\PdfSearchAI\
```

These are good choices for most users!

---

## ✨ Features You'll See

### In PdfChunkService:
- 📂 Automatic directory scanning
- 🔄 Periodic updates (configurable interval)
- ✅ Change detection (only processes new/modified files)
- 📊 Processing statistics in console

### In InternalAIAssistant:
- 🤖 AI-powered answers
- 📌 Source citations with file and page
- 📚 Complete source listings
- 🎨 Modern UI with emoji icons

---

## 🎯 Quick Command Reference

### PdfChunkService Commands:

```bash
# Run in development mode
dotnet run

# Build for release
dotnet build -c Release

# Publish for deployment
dotnet publish -c Release -o C:\PdfChunkService
```

### InternalAIAssistant Commands:

```bash
# Run the app
dotnet run

# Build for release
dotnet build -c Release
```

---

## 💡 Pro Tips

1. **Start with defaults** - They work for most users
2. **Keep database centralized** - Use Documents folder for easy backup
3. **Check sources** - Always verify important information from the cited pages
4. **Reconfigure anytime** - Just delete the config file and restart
5. **Monitor the service** - Check console output to ensure PDFs are being processed

---

## 🆘 Need More Help?

See detailed documentation:
- `PdfChunkService/README.md` - Service setup and usage
- `IMPLEMENTATION_SUMMARY_CONFIG_AND_SOURCES.md` - Technical details
- `SOURCE_DISPLAY_EXAMPLES.md` - Visual examples

---

## ✅ Success Checklist

After setup, verify:
- [ ] PdfChunkService shows "Processing: filename.pdf" messages
- [ ] Database file exists at the path you specified
- [ ] InternalAIAssistant shows database statistics on startup
- [ ] Asking questions returns answers with 📌 **Source** citations
- [ ] Both apps use the same database path

If all checks pass - you're good to go! 🎉
