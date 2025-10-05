#!/bin/bash
# Comprehensive Test Guide for Answer Accuracy Improvements
# Usage: ./test_answer_accuracy.sh

set -e

echo "============================================================"
echo " PDF Search AI - Answer Accuracy Test Guide"
echo "============================================================"
echo ""

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

print_step() {
    echo -e "${GREEN}[STEP]${NC} $1"
}

print_info() {
    echo -e "${YELLOW}[INFO]${NC} $1"
}

print_error() {
    echo -e "${RED}[ERROR]${NC} $1"
}

# Step 1: Check prerequisites
print_step "Checking prerequisites..."

# Check if Python3 is available
if command -v python3 &> /dev/null; then
    echo "  ✓ Python3 is installed"
else
    print_error "Python3 is not installed. Please install it first."
    exit 1
fi

# Check if dotnet is available
if command -v dotnet &> /dev/null; then
    echo "  ✓ .NET SDK is installed"
    DOTNET_VERSION=$(dotnet --version)
    echo "    Version: $DOTNET_VERSION"
else
    print_error ".NET SDK is not installed. Please install .NET 8.0 or higher."
    exit 1
fi

echo ""

# Step 2: Create test documents
print_step "Creating test documents..."

# Run Python test script to generate test data
python3 /tmp/test_accuracy.py

echo ""

# Step 3: Display test document structure
print_step "Test document structure:"

echo ""
echo "  English Documents:"
ls -lh /tmp/test-pdfs/english/ 2>/dev/null || echo "    (No English documents)"

echo ""
echo "  German Documents:"
ls -lh /tmp/test-pdfs/german/ 2>/dev/null || echo "    (No German documents)"

echo ""
echo ""

# Step 4: Instructions for manual testing
print_step "Manual Testing Instructions"
echo ""
echo "Since this is a WPF application, some steps require manual execution:"
echo ""

print_info "PHASE 1: Process Test Documents"
echo ""
echo "1. Navigate to PdfChunkService directory:"
echo "   cd PdfChunkService"
echo ""
echo "2. Run the service in interactive mode:"
echo "   dotnet run"
echo ""
echo "3. Configure the service:"
echo "   - Documents directory: /tmp/test-pdfs"
echo "   - Database path: /tmp/test_database.db"
echo "   - Chunk size: 500"
echo ""
echo "4. Wait for processing to complete"
echo "   Expected: ~4-6 documents processed"
echo ""
echo "5. Verify database:"
echo "   - Check for successful processing messages"
echo "   - Note the number of chunks created"
echo ""

echo ""
print_info "PHASE 2: Test with InternalAIAssistant"
echo ""
echo "1. Navigate to InternalAIAssistant directory:"
echo "   cd ../InternalAIAssistant"
echo ""
echo "2. Start Ollama (in a separate terminal):"
echo "   ollama serve"
echo ""
echo "3. Verify phi3 model is available:"
echo "   ollama list"
echo "   (Should show phi3:latest)"
echo ""
echo "4. Run InternalAIAssistant:"
echo "   dotnet run"
echo ""
echo "5. Configure database connection:"
echo "   - Browse to: /tmp/test_database.db"
echo "   - Click 'Save Configuration'"
echo ""
echo "6. Verify connection:"
echo "   - Should show: 'Connected to database. Found X chunks from Y files.'"
echo ""

echo ""
print_info "PHASE 3: Execute Test Cases"
echo ""
echo "Test each question below and record the results:"
echo ""

# English test cases
echo "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"
echo "ENGLISH TEST CASES"
echo "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"
echo ""

cat << 'EOF'
┌─────────────────────────────────────────────────────────────┐
│ Test 1: Definition Question                                │
└─────────────────────────────────────────────────────────────┘

Question: What is dependency injection?

Expected Answer Should Include:
  ✓ Definition of DI
  ✓ Mention of IoC (Inversion of Control)
  ✓ Key benefits (coupling, reusability, testability)
  ✓ Types (Constructor, Setter, Interface injection)

Evaluation:
  [ ] Answer is in English
  [ ] Contains at least 3 key concepts
  [ ] Answer is comprehensive (>500 characters)
  [ ] No hallucinated information
  
Score: ____ / 100

┌─────────────────────────────────────────────────────────────┐
│ Test 2: Process Question                                   │
└─────────────────────────────────────────────────────────────┘

Question: How does the service locator pattern work?

Expected Answer Should Include:
  ✓ Central registry concept
  ✓ Service lookup process
  ✓ Benefits (centralized management, reduced coupling)
  ✓ Runtime configuration

Evaluation:
  [ ] Answer is in English
  [ ] Describes the pattern workflow
  [ ] Mentions registry/lookup mechanism
  [ ] Explains benefits
  
Score: ____ / 100

┌─────────────────────────────────────────────────────────────┐
│ Test 3: Benefits Question                                  │
└─────────────────────────────────────────────────────────────┘

Question: What are the benefits of using interfaces?

Expected Answer Should Include:
  ✓ Polymorphism/abstraction
  ✓ Reduced coupling/decoupling
  ✓ Testing support (mocking)
  ✓ Multiple implementations
  ✓ DI/IoC support

Evaluation:
  [ ] Answer is in English
  [ ] Lists at least 4 benefits
  [ ] Provides brief explanations
  [ ] Well-structured response
  
Score: ____ / 100

EOF

echo ""
echo "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"
echo "GERMAN TEST CASES"
echo "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"
echo ""

cat << 'EOF'
┌─────────────────────────────────────────────────────────────┐
│ Test 4: Definitionsfrage                                   │
└─────────────────────────────────────────────────────────────┘

Frage: Was ist Dependency Injection?

Erwartete Antwort sollte enthalten:
  ✓ Definition von DI (auf Deutsch)
  ✓ Erwähnung von IoC (Umkehrung der Kontrolle)
  ✓ Hauptvorteile (Kopplung, Wiederverwendbarkeit, Testbarkeit)
  ✓ Arten (Konstruktor-, Setter-, Schnittstellen-Injektion)

Bewertung:
  [ ] Antwort ist auf Deutsch
  [ ] Enthält mindestens 3 Kernkonzepte
  [ ] Antwort ist umfassend (>500 Zeichen)
  [ ] Keine halluzinierten Informationen
  
Punktzahl: ____ / 100

┌─────────────────────────────────────────────────────────────┐
│ Test 5: Prozessfrage                                       │
└─────────────────────────────────────────────────────────────┘

Frage: Wie funktioniert das Service Locator Muster?

Erwartete Antwort sollte enthalten:
  ✓ Zentrales Register-Konzept
  ✓ Dienst-Suchprozess
  ✓ Vorteile (zentralisierte Verwaltung, reduzierte Kopplung)
  ✓ Laufzeit-Konfiguration

Bewertung:
  [ ] Antwort ist auf Deutsch
  [ ] Beschreibt den Muster-Workflow
  [ ] Erwähnt Register/Such-Mechanismus
  [ ] Erklärt Vorteile
  
Punktzahl: ____ / 100

┌─────────────────────────────────────────────────────────────┐
│ Test 6: Vorteilsfrage                                      │
└─────────────────────────────────────────────────────────────┘

Frage: Was sind die Vorteile von Schnittstellen?

Erwartete Antwort sollte enthalten:
  ✓ Polymorphie/Abstraktion
  ✓ Reduzierte Kopplung/Entkopplung
  ✓ Test-Unterstützung (Mocking)
  ✓ Mehrere Implementierungen
  ✓ DI/IoC-Unterstützung

Bewertung:
  [ ] Antwort ist auf Deutsch
  [ ] Listet mindestens 4 Vorteile auf
  [ ] Bietet kurze Erklärungen
  [ ] Gut strukturierte Antwort
  
Punktzahl: ____ / 100

EOF

echo ""
print_info "PHASE 4: Calculate Results"
echo ""
echo "After completing all tests:"
echo ""
echo "1. Calculate average score:"
echo "   Average = (Test1 + Test2 + Test3 + Test4 + Test5 + Test6) / 6"
echo ""
echo "2. Interpret results:"
echo "   • 90-100: Excellent - System working perfectly"
echo "   • 80-89:  Very Good - Minor improvements possible"
echo "   • 70-79:  Good - Acceptable performance"
echo "   • 60-69:  Fair - Needs improvement"
echo "   • <60:    Poor - Significant issues"
echo ""
echo "3. Document any issues found"
echo ""

echo ""
print_info "PHASE 5: Performance Testing"
echo ""
echo "For each test, also measure:"
echo ""
echo "1. Response Time:"
echo "   - Start timer when clicking 'Send'"
echo "   - Stop timer when answer appears"
echo "   - Expected: 10-20 seconds"
echo ""
echo "2. Source Attribution:"
echo "   - Verify sources are listed"
echo "   - Check page numbers are correct"
echo ""
echo "3. Context Relevance:"
echo "   - Answer should match question specificity"
echo "   - No generic/off-topic responses"
echo ""

echo ""
print_step "Debug Logging (Optional)"
echo ""
echo "To enable detailed search debugging:"
echo ""
echo "1. In AIAssistant initialization code, add:"
echo "   aiAssistant.EnableDebugLogging = true;"
echo ""
echo "2. Run tests again"
echo ""
echo "3. Check search-debug.log for:"
echo "   - Query term extraction"
echo "   - Chunk matching scores"
echo "   - Relevance ranking details"
echo ""

echo ""
print_step "Comparison Testing (Optional)"
echo ""
echo "To compare before/after improvements:"
echo ""
echo "1. Checkout previous commit:"
echo "   git checkout HEAD~1"
echo ""
echo "2. Run all 6 tests and record scores"
echo ""
echo "3. Checkout current commit:"
echo "   git checkout -"
echo ""
echo "4. Run all 6 tests again and record scores"
echo ""
echo "5. Compare results:"
echo "   - Calculate improvement percentage"
echo "   - Note specific improvements in answer quality"
echo ""

echo ""
echo "============================================================"
echo " Test Checklist Summary"
echo "============================================================"
echo ""
cat << 'EOF'
Prerequisites:
  [ ] Python3 installed
  [ ] .NET 8.0+ installed
  [ ] Ollama installed and running
  [ ] phi3 model downloaded

Test Documents:
  [ ] Test documents created in /tmp/test-pdfs/
  [ ] Documents processed by PdfChunkService
  [ ] Database populated with chunks

Application Setup:
  [ ] InternalAIAssistant running
  [ ] Database connected
  [ ] Chunks loaded successfully

Test Execution:
  [ ] Test 1 (English): Definition question
  [ ] Test 2 (English): Process question
  [ ] Test 3 (English): Benefits question
  [ ] Test 4 (German): Definitionsfrage
  [ ] Test 5 (German): Prozessfrage
  [ ] Test 6 (German): Vorteilsfrage

Results Analysis:
  [ ] Average score calculated
  [ ] Performance measured
  [ ] Issues documented
  [ ] Results compared with expectations

Optional:
  [ ] Debug logging enabled and reviewed
  [ ] Before/after comparison completed
  [ ] Performance benchmarks recorded
EOF

echo ""
echo "============================================================"
echo ""
print_info "Test guide completed. Follow the phases above to test the improvements."
echo ""
echo "For questions or issues, refer to ACCURACY_IMPROVEMENTS.md"
echo ""
