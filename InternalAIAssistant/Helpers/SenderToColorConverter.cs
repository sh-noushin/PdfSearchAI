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
            if (string.IsNullOrWhiteSpace(sender)) return new SolidColorBrush(Color.FromRgb(220, 220, 220));
            if (sender == "User") return new SolidColorBrush(Color.FromRgb(255, 255, 255)); // white bubble
            if (sender == "AI") return new SolidColorBrush(Color.FromRgb(200, 230, 255)); // light blue bubble
            return new SolidColorBrush(Color.FromRgb(220, 220, 220)); // default gray
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
