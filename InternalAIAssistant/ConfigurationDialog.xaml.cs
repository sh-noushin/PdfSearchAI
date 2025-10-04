using System.Windows;
using Microsoft.Win32;
using System.IO;
using Microsoft.EntityFrameworkCore;
using InternalAIAssistant.Data;

namespace InternalAIAssistant
{
    public partial class ConfigurationDialog : Window
    {
        public string DatabasePath { get; private set; }

        public ConfigurationDialog()
        {
            InitializeComponent();
            
            // Clear the text box - user must select a file
            DatabasePathTextBox.Text = "Please select a database file...";
            DatabasePath = string.Empty;
        }

        private void BrowseButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog
            {
                Title = "Select Existing Database File",
                Filter = "Database Files (*.db)|*.db|All Files (*.*)|*.*",
                DefaultExt = ".db",
                CheckFileExists = true,
                CheckPathExists = true,
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
            };

            if (dialog.ShowDialog() == true)
            {
                // Validate the database, but do not show confirmation dialog
                if (ValidateDatabase(dialog.FileName, showConfirmation: false))
                {
                    DatabasePathTextBox.Text = dialog.FileName;
                    DatabasePath = dialog.FileName;
                }
            }
        }

        private bool ValidateDatabase(string databasePath, bool showConfirmation = true)
        {
            try
            {
                var connectionString = $"Data Source={databasePath}";
                var options = new DbContextOptionsBuilder<PdfChunkDbContext>()
                    .UseSqlite(connectionString)
                    .Options;

                using var context = new PdfChunkDbContext(options);
                // Check if database can be opened and has required tables
                if (context.Database.CanConnect())
                {
                    // Try to query the tables to ensure they exist and have data
                    var fileCount = context.Files.Count();
                    var chunkCount = context.Chunks.Count();
                    if (showConfirmation)
                    {
                        MessageBox.Show(
                            $"Database validated successfully!\n\nFound:\n- {fileCount} files\n- {chunkCount} chunks",
                            "Database Validation",
                            MessageBoxButton.OK,
                            MessageBoxImage.Information
                        );
                    }
                    return true;
                }
                else
                {
                    MessageBox.Show(
                        "Cannot connect to the selected database file. Please check if the file is valid and not corrupted.",
                        "Database Error",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error
                    );
                    return false;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Error validating database: {ex.Message}\n\nThis may not be a valid PDF chunks database file.",
                    "Database Validation Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
                return false;
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(DatabasePath) || DatabasePath == "Please select a database file...")
            {
                MessageBox.Show("Please select a database file.", "No Database Selected", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!File.Exists(DatabasePath))
            {
                MessageBox.Show("The selected database file does not exist.", "File Not Found", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Validate the database one more time before accepting
            if (!ValidateDatabase(DatabasePath))
            {
                return;
            }

            DialogResult = true;
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
