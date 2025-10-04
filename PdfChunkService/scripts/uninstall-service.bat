@echo off
echo Uninstalling PDF Chunk Service...

REM Stop the service if running
echo Stopping service...
sc stop "PDF Chunk Service"

REM Delete the service
echo Removing service...
sc delete "PDF Chunk Service"
if %ERRORLEVEL% neq 0 (
    echo Service removal failed!
    pause
    exit /b 1
)

echo Service uninstalled successfully!
echo Note: Application files in C:\PdfChunkService were not removed.

pause