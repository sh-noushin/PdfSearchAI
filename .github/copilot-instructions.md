# PdfSearchAI Copilot Instructions

## Project Architecture

This is a two-component PDF search and AI assistant system:

1. **InternalAIAssistant** - WPF desktop app providing RAG-based chat interface
2. **PdfChunkService** - Windows service for automated PDF processing and database storage

### Component Boundaries

- **WPF App (`InternalAIAssistant/`)**: Real-time document parsing, embedding generation, and chat UI
- **Windows Service (`PdfChunkService/`)**: Background PDF monitoring, chunking, and persistence
- **Data Flow**: Service processes PDFs → Database → WPF app queries for RAG context

## Key Dependencies & Integration Points

### External Services
- **Ollama** (localhost:11434): LLM inference and embeddings via `OllamaSharp`
- **Qdrant**: Vector database for semantic search capabilities
- **SQL Server/SQLite**: Structured storage for file metadata and text chunks

### PDF Processing Stack
- **PdfPig**: Primary PDF text extraction (both projects)
- **PdfSharp**: Alternative PDF processing (service only)
- **Tesseract**: OCR capability for scanned documents

## Critical Development Workflows

### WPF App Development
```bash
# Run with hardcoded document folder
dotnet run --project InternalAIAssistant
# Documents indexed from: C:\Users\admin\Nooshin\docs
```

### Windows Service Development
```bash
# Interactive mode for development/testing
dotnet run --project PdfChunkService/PdfChunkService
# Production: Use scripts/install-service.bat
```

### Service Installation Pattern
```bash
# Automated via scripts/install-service.bat
dotnet publish --configuration Release --output "C:\PdfChunkService"
sc create "PDF Chunk Service" binPath="C:\PdfChunkService\PdfChunkService.exe"
```

## Project-Specific Patterns

### Document Processing Architecture
- **Dual Processing**: WPF app does real-time parsing; service does background batch processing
- **Chunk Strategy**: Service creates database chunks; WPF creates in-memory `DocumentChunk` objects
- **File Change Detection**: Service uses MD5 hash + modification time for incremental processing

### Search Implementation
- **Hybrid Search**: Simple keyword search (`SimpleSearchService`) + semantic search capability
- **Context Limitation**: Responses truncated to 1500 chars for Ollama compatibility
- **Embedding Model**: Mistral model via Ollama for both text generation and embeddings

### Configuration Patterns
- **Service Config**: JSON file (`service-config.json`) with interactive first-run setup
- **WPF Hardcoded**: Document path hardcoded in `MainWindow.xaml.cs` constructor
- **Database**: Entity Framework with SQLite (service) + LocalDB support

### MVVM Implementation
- **Single ViewModel**: `ChatViewModel` handles all UI logic and AI interaction
- **Command Pattern**: `RelayCommand` for WPF command binding
- **Async UI**: Busy indicators during AI processing with timeout handling

## File Organization Conventions

### Services Layer Pattern
```
Services/
├── AIAssistant.cs          # LLM integration and RAG logic
├── DocumentIndexer.cs      # Real-time document processing
├── EmbeddingService.cs     # Ollama embedding generation
└── SemanticSearchService.cs # Vector similarity search
```

### Entity Framework Pattern
```
Data/PdfChunkDbContext.cs   # EF context with unique constraints
Models/FileEntity.cs        # File metadata with hash tracking
Models/ChunkEntity.cs       # Text chunks with cascade delete
```

### Testing Structure
- Unit tests in `PdfChunkService.Tests/`
- Test database: In-memory SQLite for isolated testing
- Service testing: `PdfProcessingServiceTests.cs` for core workflow validation

## Build & Deployment Notes

- **Target Framework**: .NET 8.0 (Windows-specific for WPF/Windows Service)
- **Build Issues**: Known MSBuild .resx file warnings (non-blocking)
- **Service Deployment**: Requires administrator privileges for Windows service installation
- **Log Management**: Serilog with daily file rotation in `logs/` directory