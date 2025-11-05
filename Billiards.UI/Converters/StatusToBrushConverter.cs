using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace Billiards.UI.Converters;

public class StatusToBrushConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value == null)
            return Brushes.Gray;

        string status = value.ToString() ?? string.Empty;

        return status switch
        {
            "Free" => Brushes.Green,
            "InUse" => Brushes.Red,
            "Reserved" => Brushes.Blue,
            "Maintenance" => Brushes.Orange,
            _ => Brushes.Gray
        };
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

