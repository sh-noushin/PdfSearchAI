using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace InternalAIAssistant.Helpers
{
    public class SenderToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var sender = value as string;
            if (string.IsNullOrWhiteSpace(sender)) return new SolidColorBrush(Color.FromRgb(45, 45, 48)); // dark gray
            if (sender == "User") return new SolidColorBrush(Color.FromRgb(30, 30, 30)); // darker bubble for user
            if (sender == "AI") return new SolidColorBrush(Color.FromRgb(37, 37, 38)); // slightly lighter for AI
            if (sender == "System") return new SolidColorBrush(Color.FromRgb(45, 45, 48)); // system messages
            return new SolidColorBrush(Color.FromRgb(45, 45, 48)); // default dark gray
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
