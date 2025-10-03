using System;
using System.IO;
using System.Text.Json;

namespace PdfChunkService.Configuration
{
    /// <summary>
    /// Configuration model for PdfChunkService
    /// </summary>
    public class ServiceConfiguration
    {
        /// <summary>
        /// Directory to scan for PDF files
        /// </summary>
        public string DocumentsDirectory { get; set; } = string.Empty;

        /// <summary>
        /// Full path to the database file
        /// </summary>
        public string DatabasePath { get; set; } = string.Empty;

        /// <summary>
        /// How often to scan for new/updated files (in days)
        /// </summary>
        public int ScanIntervalDays { get; set; } = 3;

        /// <summary>
        /// Enable OCR for scanned PDFs
        /// </summary>
        public bool EnableOCR { get; set; } = false;
    }
}
