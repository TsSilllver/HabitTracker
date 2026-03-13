using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace HabitTracker.App.Converters
{
    public class BoolToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool completed && completed)
            {
                // Возвращаем кисть из ресурсов приложения
                return Application.Current.TryFindResource("CompletedDayBrush") as SolidColorBrush
                       ?? new SolidColorBrush(Colors.DodgerBlue);
            }
            else
            {
                return Application.Current.TryFindResource("IncompleteDayBrush") as SolidColorBrush
                       ?? new SolidColorBrush(Colors.Gray);
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}