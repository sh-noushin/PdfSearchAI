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

    /// <summary>
    /// Manages loading and saving service configuration
    /// </summary>
    public static class ConfigurationManager
    {
        private static readonly string ConfigDirectory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "PdfSearchAI"
        );

        private static readonly string ConfigFilePath = Path.Combine(ConfigDirectory, "service-config.json");

        /// <summary>
        /// Get existing configuration or create new one via user prompts
        /// </summary>
        public static ServiceConfiguration GetOrCreateConfiguration()
        {
            if (File.Exists(ConfigFilePath))
            {
                return LoadConfiguration();
            }

            return CreateConfigurationInteractive();
        }

        /// <summary>
        /// Load configuration from file
        /// </summary>
        private static ServiceConfiguration LoadConfiguration()
        {
            try
            {
                var json = File.ReadAllText(ConfigFilePath);
                return JsonSerializer.Deserialize<ServiceConfiguration>(json) ?? new ServiceConfiguration();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading configuration: {ex.Message}");
                Console.WriteLine("Creating new configuration...");
                return CreateConfigurationInteractive();
            }
        }

        /// <summary>
        /// Create configuration interactively
        /// </summary>
        private static ServiceConfiguration CreateConfigurationInteractive()
        {
            Console.WriteLine();
            Console.WriteLine("=== PDF Chunk Service - First Run Configuration ===");
            Console.WriteLine();

            // Documents directory
            Console.WriteLine("Enter the directory to scan for PDF files:");
            Console.WriteLine($"(Default: {GetDefaultDocumentsDirectory()})");
            Console.Write("> ");
            var documentsDir = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(documentsDir))
            {
                documentsDir = GetDefaultDocumentsDirectory();
            }

            // Database path
            Console.WriteLine();
            Console.WriteLine("Enter the database file path:");
            Console.WriteLine($"(Default: {GetDefaultDatabasePath()})");
            Console.Write("> ");
            var dbPath = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(dbPath))
            {
                dbPath = GetDefaultDatabasePath();
            }

            // Scan interval
            Console.WriteLine();
            Console.Write("Scan interval in days (default 3): ");
            var intervalInput = Console.ReadLine();
            var scanInterval = int.TryParse(intervalInput, out var days) && days > 0 ? days : 3;

            var config = new ServiceConfiguration
            {
                DocumentsDirectory = documentsDir,
                DatabasePath = dbPath,
                ScanIntervalDays = scanInterval,
                EnableOCR = false
            };

            SaveConfiguration(config);

            Console.WriteLine();
            Console.WriteLine("Configuration saved successfully!");
            Console.WriteLine();

            return config;
        }

        /// <summary>
        /// Save configuration to file
        /// </summary>
        public static void SaveConfiguration(ServiceConfiguration config)
        {
            try
            {
                // Ensure directory exists
                Directory.CreateDirectory(ConfigDirectory);

                // Ensure target directories exist
                if (!string.IsNullOrEmpty(config.DocumentsDirectory))
                {
                    Directory.CreateDirectory(config.DocumentsDirectory);
                }

                var dbDir = Path.GetDirectoryName(config.DatabasePath);
                if (!string.IsNullOrEmpty(dbDir))
                {
                    Directory.CreateDirectory(dbDir);
                }

                // Save configuration
                var json = JsonSerializer.Serialize(config, new JsonSerializerOptions
                {
                    WriteIndented = true
                });

                File.WriteAllText(ConfigFilePath, json);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving configuration: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Get default documents directory
        /// </summary>
        private static string GetDefaultDocumentsDirectory()
        {
            return Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                "PDFs"
            );
        }

        /// <summary>
        /// Get default database path
        /// </summary>
        private static string GetDefaultDatabasePath()
        {
            return Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                "PdfSearchAI",
                "pdfchunks.db"
            );
        }

        /// <summary>
        /// Check if configuration exists
        /// </summary>
        public static bool HasConfiguration()
        {
            return File.Exists(ConfigFilePath);
        }
    }
}
