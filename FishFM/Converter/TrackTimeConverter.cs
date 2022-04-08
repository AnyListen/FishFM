using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace FishFM.Converter;

public class TrackTimeConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return double.TryParse(value.ToString(), out var result) ? SecondsToTime((int) result) : "00:00:00";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return 0;
    }
    
    private static string SecondsToTime(int total)
    {
        var hour = total / 3600;
        var min = (total % 3600) / 60;
        var sec = (total % 3600) % 60;
        return hour.ToString().PadLeft(2, '0') + ":" + min.ToString().PadLeft(2, '0') + ":" +
               sec.ToString().PadLeft(2, '0');
    }
}