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
            // Always show configuration dialog at startup
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

    private void ChangeDatabase_Click(object sender, RoutedEventArgs e)
    {
        var configDialog = new ConfigurationDialog();
        var result = configDialog.ShowDialog();
        
        if (result == true)
        {
            // Save new settings
            var settings = new UserSettings 
            { 
                DatabasePath = configDialog.DatabasePath 
            };
            SettingsManager.SaveSettings(settings);

            // Show restart message
            var restartResult = MessageBox.Show(
                "Database configuration updated. The application needs to restart to apply the changes.\n\nRestart now?",
                "Restart Required",
                MessageBoxButton.YesNo,
                MessageBoxImage.Information
            );

            if (restartResult == MessageBoxResult.Yes)
            {
                // Restart the application
                System.Diagnostics.Process.Start(Application.ResourceAssembly.Location);
                Application.Current.Shutdown();
            }
        }
    }

    private void Exit_Click(object sender, RoutedEventArgs e)
    {
        Application.Current.Shutdown();
    }

    private void About_Click(object sender, RoutedEventArgs e)
    {
        MessageBox.Show(
            "PDF Search AI Assistant\n\nA desktop application for searching and querying PDF documents using AI.\n\n" +
            "This application uses a database of PDF chunks created by the PdfChunkService to provide intelligent responses to your questions.",
            "About PDF Search AI",
            MessageBoxButton.OK,
            MessageBoxImage.Information
        );
    }
}