using System.IO;
using System.Text.Json;

namespace InternalAIAssistant.Helpers
{
    public class UserSettings
    {
        public string DatabasePath { get; set; } = string.Empty;
    }

    public static class SettingsManager
    {
        private static readonly string SettingsDirectory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "PdfSearchAI"
        );
        
        private static readonly string SettingsFilePath = Path.Combine(SettingsDirectory, "user-settings.json");

        public static UserSettings LoadSettings()
        {
            try
            {
                if (File.Exists(SettingsFilePath))
                {
                    var json = File.ReadAllText(SettingsFilePath);
                    return JsonSerializer.Deserialize<UserSettings>(json) ?? new UserSettings();
                }
            }
            catch
            {
                // If there's any error loading settings, return default
            }

            return new UserSettings();
        }

        public static void SaveSettings(UserSettings settings)
        {
            try
            {
                if (!Directory.Exists(SettingsDirectory))
                {
                    Directory.CreateDirectory(SettingsDirectory);
                }

                var json = JsonSerializer.Serialize(settings, new JsonSerializerOptions 
                { 
                    WriteIndented = true 
                });
                
                File.WriteAllText(SettingsFilePath, json);
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(
                    $"Failed to save settings: {ex.Message}", 
                    "Settings Error", 
                    System.Windows.MessageBoxButton.OK, 
                    System.Windows.MessageBoxImage.Warning
                );
            }
        }

        public static bool HasConfiguration()
        {
            var settings = LoadSettings();
            return !string.IsNullOrWhiteSpace(settings.DatabasePath);
        }
    }
}
