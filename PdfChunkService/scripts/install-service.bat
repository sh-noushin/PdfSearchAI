@echo off
echo Installing PDF Chunk Service...

REM Build and publish the application
echo Building application...
dotnet build --configuration Release
if %ERRORLEVEL% neq 0 (
    echo Build failed!
    pause
    exit /b 1
)

echo Publishing application...
dotnet publish --configuration Release --output "C:\PdfChunkService"
if %ERRORLEVEL% neq 0 (
    echo Publish failed!
    pause
    exit /b 1
)

REM Install the Windows service
echo Installing Windows service...
sc create "PDF Chunk Service" binPath="C:\PdfChunkService\PdfChunkService.exe" start= auto
if %ERRORLEVEL% neq 0 (
    echo Service installation failed!
    pause
    exit /b 1
)

sc description "PDF Chunk Service" "Monitors PDF files and extracts text chunks to database"

echo Service installed successfully!
echo To start the service, run: sc start "PDF Chunk Service"
echo To check status, run: sc query "PDF Chunk Service"

pause