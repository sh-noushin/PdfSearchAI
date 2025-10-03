//using InternalAIAssistant.Helpers;
//using InternalAIAssistant.Models;
//using InternalAIAssistant.Services;
//using System.Collections.ObjectModel;
//using System.ComponentModel;
//using System.Runtime.CompilerServices;
//using System.Windows.Input;

//namespace InternalAIAssistant.ViewModels;

//public class ChatViewModel : INotifyPropertyChanged
//{
//    private readonly Services.AIAssistant _assistant;

//    public ObservableCollection<ChatMessage> Messages { get; } = new ObservableCollection<ChatMessage>();

//    private string _userInput;
//    public string UserInput
//    {
//        get => _userInput;
//        set { _userInput = value; OnPropertyChanged(); }
//    }

//    private bool _isBusy;
//    public bool IsBusy
//    {
//        get => _isBusy;
//        set { _isBusy = value; OnPropertyChanged(); }
//    }

//    public ICommand SendCommand { get; }

//    public ChatViewModel(Services.AIAssistant assistant)
//    {
//        _assistant = assistant;
//        SendCommand = new RelayCommand(async _ => await SendAsync(), _ => !string.IsNullOrWhiteSpace(UserInput) && !IsBusy);
//    }

//    private async Task SendAsync()
//    {
//        var question = UserInput;
//        Messages.Add(new ChatMessage { Sender = "User", Message = question });
//        UserInput = string.Empty;
//        IsBusy = true;
//        try
//        {
//            var answer = await _assistant.AskAsync(question);
//            Messages.Add(new ChatMessage { Sender = "AI", Message = answer });
//        }
//        finally
//        {
//            IsBusy = false;
//        }
//    }

//    public event PropertyChangedEventHandler PropertyChanged;
//    protected void OnPropertyChanged([CallerMemberName] string name = null) =>
//        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
//}




//using InternalAIAssistant.Helpers;
//using InternalAIAssistant.Models;
//using InternalAIAssistant.Services;
//using System.Collections.ObjectModel;
//using System.ComponentModel;
//using System.Runtime.CompilerServices;
//using System.Threading.Tasks;
//using System.Windows.Input;

//namespace InternalAIAssistant.ViewModels
//{
//    public class ChatViewModel : INotifyPropertyChanged
//    {
//        private readonly AIAssistant _assistant;

//        public ObservableCollection<ChatMessage> Messages { get; } = new ObservableCollection<ChatMessage>();

//        private string _userInput;
//        public string UserInput
//        {
//            get => _userInput;
//            set { _userInput = value; OnPropertyChanged(); }
//        }

//        private bool _isBusy;
//        public bool IsBusy
//        {
//            get => _isBusy;
//            set { _isBusy = value; OnPropertyChanged(); }
//        }

//        public ICommand SendCommand { get; }

//        public ChatViewModel(AIAssistant assistant)
//        {
//            _assistant = assistant;
//            SendCommand = new RelayCommand(async _ => await SendAsync(), _ => !string.IsNullOrWhiteSpace(UserInput) && !IsBusy);
//        }

//        private async Task SendAsync()
//        {
//            var question = UserInput;
//            Messages.Add(new ChatMessage { Sender = "User", Message = question });
//            UserInput = string.Empty;
//            IsBusy = true;
//            try
//            {
//                var (answer, sources) = await _assistant.AskAsync(question);
//                Messages.Add(new ChatMessage { Sender = "AI", Message = answer });
//                if (!string.IsNullOrWhiteSpace(sources))
//                    Messages.Add(new ChatMessage { Sender = "AI", Message = $"Sources:\n{sources}" });
//            }
//            finally
//            {
//                IsBusy = false;
//            }
//        }

//        public event PropertyChangedEventHandler PropertyChanged;
//        protected void OnPropertyChanged([CallerMemberName] string name = null) =>
//            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
//    }
//}





using InternalAIAssistant.Helpers;
using InternalAIAssistant.Models;
using InternalAIAssistant.Services;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;

namespace InternalAIAssistant.ViewModels
{
    public class ChatViewModel : INotifyPropertyChanged
    {
        private readonly AIAssistant _assistant;

        public ObservableCollection<ChatMessage> Messages { get; } = new ObservableCollection<ChatMessage>();

        private string _userInput = string.Empty;
        public string UserInput
        {
            get => _userInput;
            set { _userInput = value; OnPropertyChanged(); }
        }

        private bool _isBusy;
        public bool IsBusy
        {
            get => _isBusy;
            set { _isBusy = value; OnPropertyChanged(); }
        }

        public ICommand SendCommand { get; }
        public ICommand SummarizeCommand { get; }

        private string _selectedFileName = string.Empty;
        public string SelectedFileName
        {
            get => _selectedFileName;
            set { _selectedFileName = value; OnPropertyChanged(); }
        }

        public ChatViewModel(AIAssistant assistant)
        {
            _assistant = assistant;
            SendCommand = new RelayCommand(async _ => await SendAsync(), _ => !string.IsNullOrWhiteSpace(UserInput) && !IsBusy);
            // Do NOT send any system/AI message on startup
        }

        private async Task SendAsync()
        {
            var question = UserInput;
            Messages.Add(new ChatMessage { Sender = "User", Message = question });
            UserInput = string.Empty;
            IsBusy = true;
            var thinkingMsg = new ChatMessage { Sender = "AI", Message = "AI is thinking..." };
            Messages.Add(thinkingMsg);
            try
            {
                var (answer, sources) = await _assistant.AskAsync(question);
                Messages.Remove(thinkingMsg);
                Messages.Add(new ChatMessage { Sender = "AI", Message = answer });
                if (!string.IsNullOrWhiteSpace(sources))
                    Messages.Add(new ChatMessage { Sender = "AI", Message = $"Sources:\n{sources}" });
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task SummarizeAsync()
        {
            IsBusy = true;
            try
            {
                var (summary, sources) = await _assistant.SummarizeDocumentAsync(SelectedFileName, maxPages: 10);
                Messages.Add(new ChatMessage { Sender = "AI", Message = $"Summary of {SelectedFileName}:\n{summary}" });
                if (!string.IsNullOrWhiteSpace(sources))
                    Messages.Add(new ChatMessage { Sender = "AI", Message = $"Sources:\n{sources}" });
            }
            finally
            {
                IsBusy = false;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}