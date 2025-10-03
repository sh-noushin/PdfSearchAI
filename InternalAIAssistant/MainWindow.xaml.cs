using InternalAIAssistant.ViewModels;
using System.Windows;

namespace InternalAIAssistant;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();

        // Index your documents at startup
        var indexer = new Services.DocumentIndexer();
        indexer.IndexFolder(@"C:\Users\admin\Nooshin\docs"); // Adjust path as needed

        // Pass document chunks to the assistant
        var assistant = new Services.AIAssistant(indexer.Chunks);

        // Pass the assistant to the ViewModel
        this.DataContext = new ChatViewModel(assistant);
    }
}