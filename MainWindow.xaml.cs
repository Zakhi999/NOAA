using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

using NOAA_Model1;
using NOAA_Model2;

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
    double _windowWidth = 0;
    double _windowHeight = 0;
    double _windBrushOpacity = 0;
    string _windBrushColor = "";
    ApiUrls? _apiUrls;
    RadialGradientBrush? _windRadialBrush;
    CancellationTokenSource _cts = new CancellationTokenSource();
    readonly WeatherService _weatherService = new WeatherService();
    public event PropertyChangedEventHandler? PropertyChanged;
    public void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        if (string.IsNullOrEmpty(propertyName)) { return; }
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

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
    /// <summary>
    /// <see cref="System.Windows.Window"/> event
    /// </summary>
    async void Window_Loaded(object sender, RoutedEventArgs e)
    {
        try
        {
            var isodt = Extensions.ConvertToLocalTime("2026-01-11T10:06:35+00:00"); // 01/11/2026 05:06:35 AM

            this.Title = $"NOAA Weather Forecast - v{App.GetCurrentAssemblyVersion()}";
            spProgress.Visibility = Visibility.Hidden;
            btnGet.Content = Constants.MainButtonText;

            // Emmaus, PA (USA)
            _latitude = ConfigManager.Get("Latitude", defaultValue: 40.539d);
            _longitude = ConfigManager.Get("Longitude", defaultValue: -75.496d);
            _windowTop = ConfigManager.Get("WindowTop", defaultValue: 200d);
            _windowLeft = ConfigManager.Get("WindowLeft", defaultValue: 250d);
            _windowWidth = ConfigManager.Get("WindowWidth", defaultValue: 1000d);
            _windowHeight = ConfigManager.Get("WindowHeight", defaultValue: 800d);
            _windBrushColor = ConfigManager.Get("WindBrushColor", defaultValue: "#1E90FF");
            _windBrushOpacity = ConfigManager.Get("WindBrushOpacity", defaultValue: 0.5d);
            _windRadialBrush = Extensions.CreateRadialBrush(_windBrushColor, _windBrushOpacity);
            if (_windRadialBrush != null)
                spBackground.DotBrush = _windRadialBrush;

            // Check if position is on any screen
            this.RestorePosition(_windowLeft, _windowTop, _windowWidth, _windowHeight);

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
            if (url != null)
            {
                var wind = await LoadWindSpeed(url.WeeklyForecast);
                await Dispatcher.InvokeAsync(() =>
                {
                    msgBar.BarText = $"🔔 Setting background wind speed to {wind}";
                    spBackground.WindBaseSpeed += wind;
                }, System.Windows.Threading.DispatcherPriority.Background);
            }
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

    /// <summary>
    /// <see cref="System.Windows.Window"/> event
    /// </summary>
    void Window_Closing(object sender, CancelEventArgs e)
    {
        ConfigManager.Set("Latitude", _latitude);
        ConfigManager.Set("Longitude", _longitude);
        ConfigManager.Set("WindowTop", value: this.Top.IsInvalid() ? 200d : this.Top);
        ConfigManager.Set("WindowLeft", value: this.Left.IsInvalid() ? 250d : this.Left);
        if (!this.Width.IsInvalid() && this.Width >= 800) { ConfigManager.Set("WindowWidth", value: this.Width); }
        else { ConfigManager.Set("WindowWidth", value: 1000); } // restore default
        if (!this.Height.IsInvalid() && this.Height >= 600) { ConfigManager.Set("WindowHeight", value: this.Height); }
        else { ConfigManager.Set("WindowHeight", value: 800); } // restore default
        ConfigManager.Set("WindBrushColor", _windBrushColor);
        ConfigManager.Set("WindBrushOpacity", _windBrushOpacity);
        _weatherService?.Dispose();
        _cts?.Cancel(); // Signal any loops/timers that it's time to shut it down.
    }

    /// <summary>
    /// <see cref="System.Windows.Controls.Button"/> event
    /// </summary>
    async void GetWeatherClick(object sender, RoutedEventArgs e)
    {
        btnGet.Content = "";
        btnGet.IsEnabled = false;
        spProgress.Visibility = Visibility.Visible;
        Status = "🔔 Fetching data…";

        // We use the lat/long to get the ZONE/GRID URL from https://api.weather.gov
        _apiUrls = await _weatherService.GetForecastUrlAsync(_latitude, _longitude);

        if (_apiUrls is null)
        {
            await Dispatcher.InvokeAsync(async () =>
            {
                msgBar.BarText = Status = $"🚨 Failed to get forecast URL, try again later.";
                await Task.Delay(500); // prevent spamming
                spProgress.Visibility = Visibility.Hidden;
                btnGet.IsEnabled = true;
                btnGet.Content = Constants.MainButtonText;
            }, System.Windows.Threading.DispatcherPriority.Background);
            return;
        }

        // Then we'll use that to fetch the detailed forecast for the week.
        await LoadForecast(_apiUrls);

        // Re-enable UI elements and update status once complete (back on the UI thread)
        await Dispatcher.InvokeAsync(async () =>
        {
            msgBar.BarText = $"🔔 Attempt completed ({DateTime.Now.ToLongTimeString()})";
            await Task.Delay(500); // prevent spamming
            spProgress.Visibility = Visibility.Hidden;
            btnGet.IsEnabled = true;
            btnGet.Content = Constants.MainButtonText;
        }, System.Windows.Threading.DispatcherPriority.Background);
    }

    /// <summary>
    /// <see cref="System.Windows.Window"/> event
    /// </summary>
    void Window_Activated(object sender, EventArgs e) => IsAnimated = true;

    /// <summary>
    /// <see cref="System.Windows.Window"/> event
    /// </summary>
    void Window_Deactivated(object sender, EventArgs e) => IsAnimated = false;

    /// <summary>
    /// <see cref="System.Windows.Window"/> event
    /// </summary>
    void Window_SizeChanged(object sender, SizeChangedEventArgs e)
    {
        if (e.NewSize.IsInvalidOrZero())
            return;

        Debug.WriteLine($"[INFO] New size: {e.NewSize.Width:N0},{e.NewSize.Height:N0}");

        // Add in some margin
        spBackground.Width = e.NewSize.Width - 10;
        spBackground.Height = e.NewSize.Height - 10;
    }
    #endregion

    #region [Service Methods]
    async Task LoadForecast(ApiUrls url)
    {
        bool getProbabilityOfPrecipitation = false;
        List<PrecipitationValue> precipAmounts = new List<PrecipitationValue>();

        try
        {
            WeatherForecastResponse? forecast = null;

            // Weekly forecast details
            if (url is null)
                forecast = await _weatherService.GetWeeklyPHIForecastAsync("34,100"); // PHI grid default
            else
                forecast = await _weatherService.GetWeeklyForecastAsync(url.WeeklyForecast, logResponse: false);

            if (forecast == null || forecast.Properties == null)
            {
                Status = $"No data to work with";
                return;
            }

            // Precipitation forecast details
            if (url is null)
                precipAmounts = await _weatherService.GetWeeklyQuantitativePrecipitationAsync("PHI", "34,100");
            else
                precipAmounts = await _weatherService.GetWeeklyQuantitativePrecipitationAsync(url.GridData);

            Debug.WriteLine($"━━◖QuantitativePrecipitation◗━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
            foreach (var item in precipAmounts)
            {
                Debug.WriteLine($"[INFO] {item.Time}  {item.Value} ");
            }
            /* [EXAMPLE]
               2026-01-12T03:00:00+00:00/PT3H  0.0 inches
               2026-01-12T06:00:00+00:00/PT6H  0.0 inches 
               2026-01-12T12:00:00+00:00/PT6H  0.0 inches 
               2026-01-12T18:00:00+00:00/PT6H  0.0 inches 
               2026-01-13T00:00:00+00:00/PT6H  0.0 inches 
               2026-01-13T06:00:00+00:00/PT6H  0.0 inches 
               2026-01-13T12:00:00+00:00/PT6H  0.0 inches 
               2026-01-13T18:00:00+00:00/PT6H  0.0 inches 
               2026-01-14T00:00:00+00:00/PT6H  0.0 inches 
               2026-01-14T06:00:00+00:00/PT6H  0.0 inches 
               2026-01-14T12:00:00+00:00/PT6H  0.0 inches 
               2026-01-14T18:00:00+00:00/PT6H  0.0 inches 
               2026-01-15T00:00:00+00:00/PT6H  0.0 inches 
               2026-01-15T06:00:00+00:00/PT6H  0.1 inches 
            */

            #region [Extras]
            if (getProbabilityOfPrecipitation)
            {
                var probabilities = await _weatherService.GetWeeklyProbabilityOfPrecipitationAsync("PHI", "34,100");
                Debug.WriteLine($"━━◖ProbabilityOfPrecipitation◗━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
                foreach (var item in probabilities)
                {
                    Debug.WriteLine($"[INFO] {item.Time}  {item.Value} ");
                }
                /* [EXAMPLE]
                   2026-01-12T03:00:00+00:00/PT14H  0 % 
                */
            }
            #endregion

            // Confirm our collections are in sync (we could add date checking to confirm match)
            if (forecast.Properties.Periods.Count == precipAmounts.Count)
            {
                for (int i = 0; i < forecast.Properties.Periods.Count; i++)
                {
                    forecast.Properties.Periods[i].PrecipitationAmount = precipAmounts[i].Value;
                }
            }
            else
            {
                for (int i = 0; i < forecast.Properties.Periods.Count; i++)
                {
                    forecast.Properties.Periods[i].PrecipitationAmount = "N/A";
                }
            }

            // Set the data source for the ItemsControl
            ForecastList.ItemsSource = forecast.Properties.Periods;

            Status = $"Forecast updated {forecast.Properties.UpdateTime} ";
            var ucode = _weatherService.GetUnitCode(forecast.Properties.Elevation.UnitCode);
            if (ucode.Equals("m", StringComparison.OrdinalIgnoreCase))
            {
                var elevation = forecast.Properties.Elevation.Value * 3.28084; // convert meters to feet
                Status2 = $"Probability of precipitation: {forecast.Properties.Periods[0].ProbabilityOfPrecipitation.Value}% (elevation {elevation:N0} feet)";
            }
            else
            {
                Status2 = $"Probability of precipitation: {forecast.Properties.Periods[0].ProbabilityOfPrecipitation.Value}% (elevation {forecast.Properties.Elevation.Value:N0} {ucode})";
            }
        }
        catch (Exception ex)
        {
            App.ShowDialog($"Error loading forecast: {ex.Message}", "Warning", assetName: "assets/error.png");
        }
    }

    async Task<double> LoadWindSpeed(string url, double factor = 4d)
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