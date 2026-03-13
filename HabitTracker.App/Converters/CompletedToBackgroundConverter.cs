using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using HabitTracker.App.ViewModels;

namespace HabitTracker.App.Converters
{
    public class CompletedToBackgroundConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is DayCompletion day)
            {
                Color color;
                try
                {
                    color = (Color)ColorConverter.ConvertFromString(day.ColorHex);
                }
                catch
                {
                    color = Colors.DodgerBlue;
                }

                if (day.Completed)
                {
                    // Выполнено: полная заливка
                    return new SolidColorBrush(color);
                }
                else
                {
                    // Не выполнено: прозрачная заливка с лёгкой подсветкой (полупрозрачный)
                    return new SolidColorBrush(Color.FromArgb(40, color.R, color.G, color.B));
                }
            }
            return Brushes.Transparent;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}