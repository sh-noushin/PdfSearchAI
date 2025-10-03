using InternalAIAssistant.ViewModels;
using InternalAIAssistant.Data;
using InternalAIAssistant.Services;
using InternalAIAssistant.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.Windows;
using System.IO;

namespace InternalAIAssistant;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();

        try 
        {
            // Check if configuration exists
            if (!SettingsManager.HasConfiguration())
            {
                var configDialog = new ConfigurationDialog();
                var result = configDialog.ShowDialog();
                
                if (result != true)
                {
                    MessageBox.Show("Configuration is required to run the application.", 
                        "Configuration Required", MessageBoxButton.OK, MessageBoxImage.Information);
                    Application.Current.Shutdown();
                    return;
                }
                
                // Save settings
                var settings = new UserSettings 
                { 
                    DatabasePath = configDialog.DatabasePath 
                };
                SettingsManager.SaveSettings(settings);
            }

            // Load settings
            var userSettings = SettingsManager.LoadSettings();
            var connectionString = $"Data Source={userSettings.DatabasePath}";
            
            if (string.IsNullOrEmpty(userSettings.DatabasePath))
            {
                MessageBox.Show("Database path not configured.", "Configuration Error", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Set up database context for SQLite
            var options = new DbContextOptionsBuilder<PdfChunkDbContext>()
                .UseSqlite(connectionString)
                .Options;

            var dbContext = new PdfChunkDbContext(options);

            // Ensure database is created
            dbContext.Database.EnsureCreated();

            // Create database service
            var databaseService = new DatabaseChunkService(dbContext);

            // Create AI assistant with database service
            var assistant = new AIAssistant(databaseService);

            // Set up ViewModel
            this.DataContext = new ChatViewModel(assistant);
            
            // Show startup message with database statistics
            LoadStatisticsAsync(databaseService);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Failed to initialize application: {ex.Message}", "Initialization Error", 
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private async void LoadStatisticsAsync(DatabaseChunkService databaseService)
    {
        try 
        {
            var (fileCount, chunkCount) = await databaseService.GetStatisticsAsync();
            var recentFiles = await databaseService.GetRecentFilesAsync(7);
            
            var viewModel = (ChatViewModel)this.DataContext;
            viewModel.Messages.Add(new Models.ChatMessage 
            { 
                Sender = "System", 
                Message = $"Connected to database. Found {chunkCount} chunks from {fileCount} files.\nRecent files (last 7 days): {recentFiles.Count}"
            });
            
            if (recentFiles.Any())
            {
                viewModel.Messages.Add(new Models.ChatMessage 
                { 
                    Sender = "System", 
                    Message = $"Recent files:\n{string.Join("\n", recentFiles.Take(5).Select(f => $"- {f}"))}"
                });
            }
        }
        catch (Exception ex)
        {
            var viewModel = (ChatViewModel)this.DataContext;
            viewModel?.Messages.Add(new Models.ChatMessage 
            { 
                Sender = "System", 
                Message = $"Warning: Could not load database statistics: {ex.Message}"
            });
        }
    }
}