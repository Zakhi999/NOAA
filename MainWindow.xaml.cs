using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace NOAA;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window, INotifyPropertyChanged
{
    #region [Properties]
    double _latitude = 0;
    double _longitude = 0;
    double _windowLeft = 0;
    double _windowTop = 0;
    double _windBrushOpacity = 0;
    string _windBrushColor = "";
    RadialGradientBrush? _windRadialBrush;
    CancellationTokenSource _cts = new CancellationTokenSource();
    readonly WeatherService _weatherService = new WeatherService();
    public event PropertyChangedEventHandler? PropertyChanged;
    public void OnPropertyChanged([CallerMemberName] string propertyName = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    bool isAnimated = false;
    public bool IsAnimated
    {
        get => isAnimated;
        set
        {
            if (isAnimated != value)
            {
                isAnimated = value;
                OnPropertyChanged();
            }
        }
    }

    string status = "";
    public string Status
    {
        get => status;
        set
        {
            if (status != value)
            {
                status = value;
                OnPropertyChanged();
            }
        }
    }

    string status2 = "";
    public string Status2
    {
        get => status2;
        set
        {
            if (status2 != value)
            {
                status2 = value;
                OnPropertyChanged();
            }
        }
    }
    #endregion

    public MainWindow()
    {
        InitializeComponent();
        
        this.DataContext = this; // ⇦ very important for INotifyPropertyChanged!

        Debug.WriteLine($"[INFO] Application version {App.GetCurrentAssemblyVersion()}");
    }

    #region [Events]
    async void GetWeatherClick(object sender, RoutedEventArgs e)
    {
        btnGet.Content = "";
        btnGet.IsEnabled = false;
        spinner1.Visibility = Visibility.Visible;
        Status = "🔔 Fetching data…";

        // We use the lat/long to get the ZONE/GRID URL from https://api.weather.gov
        var url = await _weatherService.GetForecastUrlAsync(_latitude, _longitude);

        // Then we'll use that to fetch the detailed forecast for the week.
        await LoadForecast(url);

        // Re-enable UI elements and update status once complete (back on the UI thread)
        await Dispatcher.InvokeAsync(async () =>
        {
            msgBar.BarText = $"🔔 Attempt completed ({DateTime.Now.ToLongTimeString()})";
            await Task.Delay(500); // prevent spamming
            spinner1.Visibility = Visibility.Hidden;
            btnGet.IsEnabled = true;
            btnGet.Content = Constants.MainButtonText;
        }, System.Windows.Threading.DispatcherPriority.Background);
    }

    void Window_Activated(object sender, EventArgs e) => IsAnimated = true;

    void Window_Deactivated(object sender, EventArgs e) => IsAnimated = false;

    async void Window_Loaded(object sender, RoutedEventArgs e)
    {
        try
        {
            this.Title = $"NOAA Weather Forecast - v{App.GetCurrentAssemblyVersion()}";
            spinner1.Visibility = Visibility.Hidden;
            btnGet.Content = Constants.MainButtonText;

            // Emmaus, PA (USA)
            _latitude = ConfigManager.Get("Latitude", defaultValue: 40.539d);
            _longitude = ConfigManager.Get("Longitude", defaultValue: -75.496d);
            _windowLeft = ConfigManager.Get("WindowLeft", defaultValue: 250d);
            _windowTop = ConfigManager.Get("WindowTop", defaultValue: 200d);
            _windBrushColor = ConfigManager.Get("WindBrushColor", defaultValue: "#1E90FF");
            _windBrushOpacity = ConfigManager.Get("WindBrushOpacity", defaultValue: 0.5d);
            _windRadialBrush = Extensions.CreateRadialBrush(_windBrushColor, _windBrushOpacity);
            if (_windRadialBrush != null)
                bkgnd.DotBrush = _windRadialBrush;

            // Check if position is on any screen
            this.RestorePosition(_windowLeft, _windowTop);

            Status = $"🔔 Testing internet access";
            if (!await Extensions.PingHostAsync("8.8.8.8"))
            {
                if (!await Extensions.PingHostAsync("1.1.1.1"))
                    App.ShowDialog($"🚨 You might not have internet access. Any subsequent API calls will probably fail.", "Warning", assetName: "assets/warning.png");
            }
            else
            {
                Status = $"Latitude is {_latitude}, longitude is {_longitude}. This can be adjusted in \"Settings.xml\"";
            }

            #region [Set the wind speed background effect]
            var url = await _weatherService.GetForecastUrlAsync(_latitude, _longitude);
            var wind = await LoadWindSpeed(url);
            await Dispatcher.InvokeAsync(async () => 
            {
                msgBar.BarText = $"🔔 Setting background wind speed to {wind}";
                bkgnd.WindBaseSpeed += wind; 
            }, System.Windows.Threading.DispatcherPriority.Background);
            #endregion

            // Start the background loop
            _ = Extensions.RunEveryMidnight(() => { GetWeatherClick(null, new RoutedEventArgs()); }, _cts.Token);
            //_ = Extensions.RunEveryMidnightAsync(GetWeatherClick(null, new RoutedEventArgs()), _cts.Token);
        }
        catch (Exception ex)
        {
            Extensions.WriteToLog($"Window_Loaded: {ex.Message}", level: LogLevel.ERROR);
        }
    }

    void Window_Closing(object sender, CancelEventArgs e)
    {
        ConfigManager.Set("Latitude", _latitude);
        ConfigManager.Set("Longitude", _longitude);
        ConfigManager.Set("WindowLeft", value: this.Left.IsInvalid() ? 250D : this.Left);
        ConfigManager.Set("WindowTop", value: this.Top.IsInvalid() ? 200D : this.Top);
        ConfigManager.Set("WindBrushColor", _windBrushColor);
        ConfigManager.Set("WindBrushOpacity", _windBrushOpacity);
        _weatherService?.Dispose();
        _cts?.Cancel(); // Stop the timer when app closes
    }

    void Window_SizeChanged(object sender, SizeChangedEventArgs e)
    {
        if (e.NewSize.IsInvalidOrZero())
            return;

        Debug.WriteLine($"[INFO] New size: {e.NewSize.Width:N0},{e.NewSize.Height:N0}");

        // Add in some margin
        bkgnd.Width = e.NewSize.Width - 10;
        bkgnd.Height = e.NewSize.Height - 10;
    }
    #endregion

    #region [Service Methods]
    async Task LoadForecast(string url)
    {
        try
        {
            WeatherForecastResponse? forecast = null;

            if (string.IsNullOrEmpty(url))
                forecast = await _weatherService.GetWeeklyPHIForecastAsync("34,100"); // PHI grid default
            else
                forecast = await _weatherService.GetWeeklyForecastAsync(url, logResponse: false);

            if (forecast == null || forecast.Properties == null)
            {
                Status = $"No data to work with";
                return;
            }

            // Set the data source for the ItemsControl
            ForecastList.ItemsSource = forecast.Properties.Periods;

            //Extensions.ConvertToLocalTime(forecast.Properties.UpdateTime);

            Status2 = $"Probability of precipitation: {forecast.Properties.Periods[0].ProbabilityOfPrecipitation.Value}%";

            if (forecast.Properties.Elevation.UnitCode.StartsWith("wmoUnit:m", StringComparison.OrdinalIgnoreCase))
            {
                var elevation = forecast.Properties.Elevation.Value * 3.28084; // convert meters to feet
                Status = $"Updated {forecast.Properties.UpdateTime} (elevation {elevation:N0} feet)";
            }
            else
            {
                Status = $"Updated {forecast.Properties.UpdateTime} (elevation {forecast.Properties.Elevation.Value:N0} {forecast.Properties.Elevation.UnitCode.Replace("wmoUnit:","")})";
            }
        }
        catch (Exception ex)
        {
            App.ShowDialog($"Error loading forecast: {ex.Message}", "Warning", assetName: "assets/error.png");
        }
    }

    async Task<double> LoadWindSpeed(string url, double factor = 3.0)
    {
        double result = 0d;
        try
        {
            WeatherForecastResponse? forecast = null;

            if (string.IsNullOrEmpty(url))
                forecast = await _weatherService.GetWeeklyPHIForecastAsync("34,100"); // PHI grid default
            else
                forecast = await _weatherService.GetWeeklyForecastAsync(url, logResponse: false);

            if (forecast == null || forecast.Properties == null)
                return result;

            var wind = forecast.Properties.Periods[0].WindSpeed;
            if (string.IsNullOrEmpty(wind))
                return result;

            var max = Extensions.GetMaximumNumber(wind);
            if (max > 0)
                result = (double)max;

        }
        catch (Exception ex)
        {
            Extensions.WriteToLog($"Error loading WindSpeed: {ex.Message}", level: LogLevel.ERROR);
        }
        return result * factor;
    }
    #endregion
}