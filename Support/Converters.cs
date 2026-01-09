using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace NOAA;

public class StringToBrushConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is string)
        {
            try
            {
                return new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString((string)value));
            }
            catch
            {
                return new SolidColorBrush(Colors.Gray);
            }
        }
        else
        {
            return new SolidColorBrush(Colors.Gray);
        }
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return null;
    }
}

public class BoolToReverseConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var val = (bool)value;
        return !val;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return null;
    }
}

public class BoolToVisibilityConverter : IValueConverter
{
    public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var val = (bool)value;
        if (parameter is string param && (param.ToString().Equals("inverse", StringComparison.OrdinalIgnoreCase) || param.ToString().Equals("reverse", StringComparison.OrdinalIgnoreCase) || param.ToString().Equals("opposite", StringComparison.OrdinalIgnoreCase)))
            val = !val;
        return val ? Visibility.Visible : Visibility.Hidden;
    }

    public object? ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return null;
    }
}

public class BoolToImageConverter : IValueConverter
{
    public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        try
        {
            bool enabled = (bool)value;
            if (enabled)
                return new BitmapImage(Constants.AssetNotice);
            else
                return new BitmapImage(Constants.AssetWarning);
        }
        catch (Exception ex)
        {
            Extensions.WriteToLog($"BoolToImageConverter: {ex.Message}", level: LogLevel.ERROR);
            return null;
        }
    }

    public object? ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return null;
    }
}

public class PathToImageConverter : IValueConverter
{
    public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        try
        {
            string imagePath = (string)value;
            BitmapImage imageBitmap = new BitmapImage(new Uri(imagePath, UriKind.RelativeOrAbsolute));
            return imageBitmap;
        }
        catch (Exception ex)
        {
            Extensions.WriteToLog($"PathToImageConverter: {ex.Message}");
            return null;
        }
    }

    public object? ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return null;
    }
}

public class ImagePathConverter : IValueConverter
{
    string imageDirectory = System.IO.Directory.GetCurrentDirectory();
    public string ImageDirectory
    {
        get { return imageDirectory; }
        set { imageDirectory = value; }
    }

    public object? Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
        try
        {
            string imagePath = System.IO.Path.Combine(ImageDirectory, (string)value);
            return new BitmapImage(new Uri(imagePath));
        }
        catch (Exception ex)
        {
            Extensions.WriteToLog($"ImagePathConverter: {ex.Message}");
            return null;
        }
    }

    public object? ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
        return null;
    }
}

public class WeatherIconFallbackConverter : IValueConverter
{
    public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var url = value as string;

        if (string.IsNullOrWhiteSpace(url))
            return "pack://application:,,,/Assets/logo.png";

        return url;
    }

    public object? ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return null;
    }
}
