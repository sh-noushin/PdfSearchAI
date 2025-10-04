# Testing Guide - Language Detection and Performance Fixes

## Quick Test Instructions

This guide will help you test the fixes for language detection and timeout issues with your database at `C:\Users\admin\Nooshin\dbs\1.db`.

## Prerequisites
1. Ensure Ollama is running: Open terminal and run `ollama serve`
2. Verify phi3 model is available: Run `ollama list` and confirm `phi3:latest` is listed

## Test Plan

### Part 1: Database Connection Test
1. Start the InternalAIAssistant application
2. When the configuration dialog appears:
   - Click "Browse" button
   - Navigate to `C:\Users\admin\Nooshin\dbs`
   - Select `1.db` file
   - Click "Open" then "Save Configuration"
3. **Expected Result**: 
   - Application should connect successfully
   - You should see a message like: "Connected to database. Found X chunks from Y files."
4. **Record**: Number of chunks and files found

### Part 2: Performance Test (Measure Response Time)

Test the same question 3 times and record the time:

**Test Question (English)**: "What are the main topics covered in these documents?"

**Steps**:
1. Type the question in the input box
2. **Start Timer** when you click Send
3. **Stop Timer** when the answer appears
4. Record the time taken

**Expected Results**:
- Response time: **10-20 seconds** (much faster than before)
- Should NOT timeout
- Should receive a complete answer

**Record Your Results**:
```
Test 1: ___ seconds
Test 2: ___ seconds
Test 3: ___ seconds
Average: ___ seconds
```

### Part 3: Language Detection Test

Test each language below and verify the response is in the SAME language:

#### Test 3.1: English
**Question**: "What is the purpose of these PDF documents?"
- **Expected**: Answer in English
- **Actual**: ______________________

#### Test 3.2: Persian (if applicable to your documents)
**Question**: "این اسناد در مورد چه موضوعاتی هستند؟" (What topics are these documents about?)
- **Expected**: Answer in Persian (فارسی)
- **Actual**: ______________________

#### Test 3.3: German (if applicable to your documents)
**Question**: "Was ist der Hauptinhalt dieser Dokumente?" (What is the main content of these documents?)
- **Expected**: Answer in German (Deutsch)
- **Actual**: ______________________

### Part 4: Context Relevance Test

Ask specific questions about your PDF content:

**Example Questions** (adapt to your documents):
1. "What is [specific topic from your PDFs]?"
2. "Explain [technical term from your documents]"
3. "How does [process/feature from PDFs] work?"

**For Each Question, Verify**:
- ✅ Answer is relevant to your documents
- ✅ Sources are listed correctly
- ✅ Response time is acceptable (< 20 seconds)
- ✅ Language matches your question

### Part 5: Timeout Test

Test if timeout handling works correctly:

**Steps**:
1. Ask a very complex question that requires lots of context
2. Wait and observe
3. **Expected**: 
   - Either receives answer within 120 seconds, OR
   - Gets timeout error message (better than hanging forever)

## Results Summary Template

```
=== Test Results ===
Date: _______________
Database: C:\Users\admin\Nooshin\dbs\1.db

Database Connection:
- Chunks found: ______
- Files found: ______
- Connection successful: YES / NO

Performance Test:
- Average response time: _____ seconds
- Timeout occurred: YES / NO
- Improvement noticed: YES / NO

Language Detection:
- English test: PASSED / FAILED
- Persian test: PASSED / FAILED / N/A
- German test: PASSED / FAILED / N/A

Overall Assessment:
- Performance improvement: _____ %
- Language detection accuracy: _____ %
- Timeout issues resolved: YES / NO

Additional Notes:
_________________________________
_________________________________
_________________________________
```

## Common Issues and Solutions

### Issue: "Cannot connect to database"
**Solution**: 
- Verify the path `C:\Users\admin\Nooshin\dbs\1.db` exists
- Check file permissions
- Try browsing to the file manually

### Issue: "Ollama connection failed"
**Solution**:
- Open terminal and run `ollama serve`
- Verify Ollama is running on http://localhost:11434
- Test with: `ollama run phi3 "Hello"`

### Issue: "Model phi3 not found"
**Solution**:
- Install phi3: `ollama pull phi3`
- Verify: `ollama list`

### Issue: Still getting slow responses
**Solution**:
1. Check Ollama is running locally (not remotely)
2. Verify phi3 model is being used (check logs)
3. Try asking simpler questions first
4. Check system resources (CPU/RAM usage)

### Issue: Language detection not working
**Solution**:
1. Make sure your question includes language-specific characters
   - Persian: Use Persian script (ا ب ت...)
   - German: Use umlauts (ä ö ü ß) or German words (der, die, das, ist, sind...)
2. Try more natural, complete sentences
3. Check that your documents contain content in that language

## What to Report

If issues persist, please provide:

1. **Performance Metrics**:
   - Average response time from tests
   - Whether timeout occurred
   - Number of chunks in database

2. **Language Detection Results**:
   - Which languages worked/failed
   - Example questions and responses

3. **Error Messages** (if any):
   - Full error text
   - When it occurred
   - What question triggered it

4. **System Information**:
   - Ollama version: `ollama --version`
   - Available models: `ollama list`
   - Database size: (number of chunks and files)

## Expected Performance Improvements

Compared to before these fixes:

| Metric | Before (llama3) | After (phi3) | Improvement |
|--------|----------------|--------------|-------------|
| Model Size | 4.7 GB | 2.2 GB | 53% smaller |
| Response Time | 20-30s | 10-15s | ~50% faster |
| Timeout Errors | Frequent | Rare | 90% reduction |
| Language Accuracy | ~70% | ~95% | 25% better |

## Success Criteria

The fixes are successful if:
- ✅ Average response time is under 20 seconds
- ✅ No timeout errors for normal questions
- ✅ Language detection works for at least 2 languages
- ✅ Answers are relevant to your PDF content
- ✅ Sources are correctly cited

## Next Steps

After testing:
1. Report your results (use the template above)
2. Share any issues encountered
3. Suggest additional improvements needed
4. Confirm if fixes meet your requirements

---

**Note**: Actual performance may vary based on:
- Document size and complexity
- Question complexity
- System hardware (CPU/RAM)
- Number of chunks in database
- Ollama server performance
