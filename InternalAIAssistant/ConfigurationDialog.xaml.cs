using System.Windows;
using Microsoft.Win32;
using System.IO;

namespace InternalAIAssistant
{
    public partial class ConfigurationDialog : Window
    {
        public string DatabasePath { get; private set; }

        public ConfigurationDialog()
        {
            InitializeComponent();
            
            // Set default path
            var defaultPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                "PdfSearchAI",
                "pdfchunks.db"
            );
            DatabasePathTextBox.Text = defaultPath;
            DatabasePath = defaultPath;
        }

        private void BrowseButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new SaveFileDialog
            {
                Title = "Select Database Location",
                Filter = "Database Files (*.db)|*.db|All Files (*.*)|*.*",
                DefaultExt = ".db",
                FileName = "pdfchunks.db",
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
            };

            if (dialog.ShowDialog() == true)
            {
                DatabasePathTextBox.Text = dialog.FileName;
                DatabasePath = dialog.FileName;
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(DatabasePathTextBox.Text))
            {
                MessageBox.Show("Please select a database location.", "Configuration Error", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            DatabasePath = DatabasePathTextBox.Text;

            // Ensure directory exists
            var directory = Path.GetDirectoryName(DatabasePath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                try
                {
                    Directory.CreateDirectory(directory);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Failed to create directory: {ex.Message}", "Error", 
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
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
