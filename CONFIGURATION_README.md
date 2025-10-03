# Configuration and UI Improvements - README

## Overview

This update addresses the following requirements:
1. ✅ Modern, Copilot-inspired UI for InternalAIAssistant
2. ✅ First-run configuration dialog for database path (InternalAIAssistant)
3. ✅ Template/guide for PdfChunkService configuration (service code not in repository)

## InternalAIAssistant Changes

### New Features

#### 1. First-Run Configuration Dialog
When you run InternalAIAssistant for the first time, you'll see a configuration dialog asking for:
- **Database Path**: Where to store/read the PDF chunks database

**Default Location**: `%USERPROFILE%\Documents\PdfSearchAI\pdfchunks.db`

The configuration is saved to: `%APPDATA%\PdfSearchAI\user-settings.json`

#### 2. Modern Dark Theme UI
The application now features a professional dark theme inspired by GitHub Copilot and VS Code:

**Visual Improvements:**
- Dark background (#1e1e1e) for reduced eye strain
- Professional color scheme matching modern IDEs
- Enhanced message bubbles with drop shadows
- Larger, more comfortable input area
- Modern rounded buttons with hover effects
- Better typography and spacing
- Increased window size (900x700) for better usability

**Color Palette:**
- Background: `#1e1e1e` (main window)
- Chat Area: `#252526` (message area)
- Input Area: `#2d2d2d` (textbox background)
- Accent: `#0e639c` (buttons and focus)
- Text: `#cccccc` (primary text)

### How to Use

1. **First Run**:
   - Launch InternalAIAssistant
   - Configuration dialog appears automatically
   - Click "Browse..." to select database location
   - Click "Save" to store configuration

2. **Subsequent Runs**:
   - Application loads using saved configuration
   - No configuration dialog appears
   - Database connection is automatic

3. **Reconfigure**:
   - Delete: `%APPDATA%\PdfSearchAI\user-settings.json`
   - Restart application
   - Configuration dialog appears again

### File Changes

**New Files:**
- `ConfigurationDialog.xaml` - Configuration UI
- `ConfigurationDialog.xaml.cs` - Configuration logic
- `Helpers/SettingsManager.cs` - Settings persistence

**Modified Files:**
- `MainWindow.xaml` - New dark theme UI
- `MainWindow.xaml.cs` - Configuration check on startup
- `Helpers/SenderToColorConverter.cs` - Dark theme colors

## PdfChunkService Configuration

### Status
The PdfChunkService code is not present in this repository, but based on the logs in the problem statement, it exists on the user's machine with hardcoded paths.

### What Was Provided

1. **Configuration Guide**: `PDFCHUNKSERVICE_CONFIG_GUIDE.md`
   - Detailed implementation guide
   - Console and WPF configuration examples
   - Best practices and security considerations

2. **Configuration Template**: `PdfChunkService-ConfigurationTemplate.cs`
   - Complete, ready-to-use configuration class
   - Interactive console prompts
   - Persistent storage in AppData
   - Default path handling

### How to Implement (for PdfChunkService)

1. Copy the configuration template code into your PdfChunkService project
2. Replace hardcoded paths with configuration calls:
   ```csharp
   // Old (hardcoded):
   var documentsPath = @"C:\Users\admin\Nooshin\docs";
   
   // New (configurable):
   var config = ConfigurationManager.GetOrCreateConfiguration();
   var documentsPath = config.DocumentsDirectory;
   ```

3. The configuration will be stored at:
   `%APPDATA%\PdfSearchAI\service-config.json`

### Important: Database Path Coordination

⚠️ **Critical**: Both InternalAIAssistant and PdfChunkService must use the **same database file**:

- PdfChunkService writes chunks to the database
- InternalAIAssistant reads chunks from the database
- Both must point to the same file path

**Recommended Approach:**
1. Configure PdfChunkService first (creates database)
2. Configure InternalAIAssistant with the same database path
3. Or document this requirement clearly to users

## Technical Details

### Configuration Storage

Both applications store configuration in AppData:
```
%APPDATA%\PdfSearchAI\
├── user-settings.json      (InternalAIAssistant)
└── service-config.json     (PdfChunkService)
```

### Database Location

Recommended default:
```
%USERPROFILE%\Documents\PdfSearchAI\pdfchunks.db
```

Benefits:
- Not in project directory
- User-accessible location
- Survives project updates
- Easy to backup

### Migration from Hardcoded Paths

If you previously had hardcoded paths:

1. **InternalAIAssistant**: 
   - Old: `appsettings.json` with hardcoded connection string
   - New: User-configurable via dialog, stored in AppData

2. **PdfChunkService**:
   - Old: Hardcoded `C:\Users\admin\Nooshin\docs`
   - New: User-configurable via console prompts, stored in AppData

## Testing Checklist

### InternalAIAssistant
- [ ] Configuration dialog appears on first run
- [ ] Can browse and select database path
- [ ] Configuration saves successfully
- [ ] Application starts without dialog on second run
- [ ] Database connects properly
- [ ] UI looks modern and professional
- [ ] Dark theme is applied correctly
- [ ] Messages display with proper styling

### PdfChunkService (when implemented)
- [ ] Configuration prompts appear on first run
- [ ] Can enter documents directory
- [ ] Can enter database path
- [ ] Directories are created if missing
- [ ] Configuration saves to AppData
- [ ] Service loads configuration on restart
- [ ] No prompts on subsequent runs

## Troubleshooting

### InternalAIAssistant

**Problem**: Configuration dialog doesn't appear
- **Solution**: Check if `%APPDATA%\PdfSearchAI\user-settings.json` exists. Delete it to reset.

**Problem**: Cannot find database
- **Solution**: Ensure PdfChunkService has created the database at the configured path.

**Problem**: UI looks wrong
- **Solution**: Ensure you're running on Windows. WPF requires Windows to render properly.

### PdfChunkService

**Problem**: Configuration prompts every time
- **Solution**: Check if `%APPDATA%\PdfSearchAI\service-config.json` is being created with write permissions.

**Problem**: Cannot access configured paths
- **Solution**: Verify paths exist and service has read/write permissions.

## Future Enhancements

Potential improvements for future versions:

1. **Settings Dialog**: Add menu option to reconfigure without deleting files
2. **Shared Configuration**: Create a shared config that both apps read from
3. **Path Validation**: Real-time validation of paths in configuration dialog
4. **Configuration Wizard**: Multi-step wizard for first-time setup
5. **Theme Selector**: Allow users to choose between light/dark themes

## Support

For issues or questions:
1. Check this README
2. Review `UI_CHANGES.md` for UI details
3. Review `PDFCHUNKSERVICE_CONFIG_GUIDE.md` for service configuration
4. Check application logs in the chat window
