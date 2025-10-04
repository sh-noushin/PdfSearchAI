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
            
            // Don't set a default path - user must browse for existing database
            DatabasePathTextBox.Text = "Click 'Browse...' to select your database file";
            DatabasePath = string.Empty;
        }

        private void BrowseButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog
            {
                Title = "Select Existing Database File",
                Filter = "Database Files (*.db)|*.db|All Files (*.*)|*.*",
                DefaultExt = ".db",
                FileName = "pdfchunks.db",
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                CheckFileExists = true
            };

            if (dialog.ShowDialog() == true)
            {
                DatabasePathTextBox.Text = dialog.FileName;
                DatabasePath = dialog.FileName;
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(DatabasePath) || !File.Exists(DatabasePath))
            {
                MessageBox.Show("Please browse and select an existing database file.", "Configuration Error", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
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
