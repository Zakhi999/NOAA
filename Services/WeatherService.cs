using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace NOAA;

public class WeatherService : IDisposable
{
    readonly HttpClient _http;

    public WeatherService()
    {
        _http = new HttpClient();
        _http.DefaultRequestHeaders.Add("User-Agent", "WpfWeatherApp/1.0 (account@github.com)");
        //_http.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("WpfWeatherApp", "1.0"));
    }

    public async Task<WeatherForecastResponse?> GetWeeklyForecastAsync(string url, bool logResponse = false)
    {
        try
        {
            var json = await _http.GetStringAsync(url);
            
            if (string.IsNullOrWhiteSpace(json))
            {
                App.ShowDialog($"Error loading forecast (empty response from host).", "Warning", assetName: "assets/warning.png");
                return new WeatherForecastResponse();
            }

            if (logResponse)
                Extensions.WriteToLog(json, level: LogLevel.DEBUG);

            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var forecast = JsonSerializer.Deserialize<WeatherForecastResponse>(json, options);

            Debug.WriteLine($"[INFO] Forecast generated at: {forecast.Properties.GeneratedAt}");
            Debug.WriteLine($"[INFO] Probability of precipitation: {forecast.Properties.Periods[0].ProbabilityOfPrecipitation.Value}%");

            return forecast;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[ERROR] GetWeeklyForecastAsync: {ex.Message}");
            return new WeatherForecastResponse();
        }
    }

    public async Task<WeatherForecastResponse?> GetWeeklyPHIForecastAsync(string gridPoint = "34,100")
    {
        try
        {
            // https://api.weather.gov/gridpoints/PHI/34,100/forecast
            var url = $"https://api.weather.gov/gridpoints/PHI/{gridPoint}/forecast";
            var json = await _http.GetStringAsync(url);

            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var forecast = JsonSerializer.Deserialize<WeatherForecastResponse>(json, options);

            Debug.WriteLine($"[INFO] Forecast generated at: {forecast.Properties.GeneratedAt}");
            Debug.WriteLine($"[INFO] Today's High: {forecast.Properties.Periods[0].Temperature}°F");

            return forecast;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[ERROR] GetWeeklyForecastAsync: {ex.Message}");
            return new WeatherForecastResponse();
        }
    }

    public async Task<string> GetForecastUrlAsync(double lat, double lon)
    {
        try
        {
            var pointUrl = $"https://api.weather.gov/points/{lat},{lon}";
            var pointJson = await _http.GetStringAsync(pointUrl);
            var url = GetForecastUrl(pointJson);
            return url;
            /*
            {
               "@context": [
                   "https://geojson.org/geojson-ld/geojson-context.jsonld",
                   {
                       "@version": "1.1",
                       "wx": "https://api.weather.gov/ontology#",
                       "s": "https://schema.org/",
                       "geo": "http://www.opengis.net/ont/geosparql#",
                       "unit": "http://codes.wmo.int/common/unit/",
                       "@vocab": "https://api.weather.gov/ontology#",
                       "geometry": {
                           "@id": "s:GeoCoordinates",
                           "@type": "geo:wktLiteral"
                       },
                       "city": "s:addressLocality",
                       "state": "s:addressRegion",
                       "distance": {
                           "@id": "s:Distance",
                           "@type": "s:QuantitativeValue"
                       },
                       "bearing": {
                           "@type": "s:QuantitativeValue"
                       },
                       "value": {
                           "@id": "s:value"
                       },
                       "unitCode": {
                           "@id": "s:unitCode",
                           "@type": "@id"
                       },
                       "forecastOffice": {
                           "@type": "@id"
                       },
                       "forecastGridData": {
                           "@type": "@id"
                       },
                       "publicZone": {
                           "@type": "@id"
                       },
                       "county": {
                           "@type": "@id"
                       }
                   }
               ],
               "id": "https://api.weather.gov/points/40.539,-75.496",
               "type": "Feature",
               "geometry": {
                   "type": "Point",
                   "coordinates": [
                       -75.496,
                       40.539
                   ]
               },
               "properties": {
                   "@id": "https://api.weather.gov/points/40.539,-75.496",
                   "@type": "wx:Point",
                   "cwa": "PHI",
                   "forecastOffice": "https://api.weather.gov/offices/PHI",
                   "gridId": "PHI",
                   "gridX": 34,
                   "gridY": 100,
                   "forecast": "https://api.weather.gov/gridpoints/PHI/34,100/forecast",
                   "forecastHourly": "https://api.weather.gov/gridpoints/PHI/34,100/forecast/hourly",
                   "forecastGridData": "https://api.weather.gov/gridpoints/PHI/34,100",
                   "observationStations": "https://api.weather.gov/gridpoints/PHI/34,100/stations",
                   "relativeLocation": {
                       "type": "Feature",
                       "geometry": {
                           "type": "Point",
                           "coordinates": [
                               -75.497883,
                               40.535188
                           ]
                       },
                       "properties": {
                           "city": "Emmaus",
                           "state": "PA",
                           "distance": {
                               "unitCode": "wmoUnit:m",
                               "value": 452.76004212283
                           },
                           "bearing": {
                               "unitCode": "wmoUnit:degree_(angle)",
                               "value": 20
                           }
                       }
                   },
                   "forecastZone": "https://api.weather.gov/zones/forecast/PAZ061",
                   "county": "https://api.weather.gov/zones/county/PAC077",
                   "fireWeatherZone": "https://api.weather.gov/zones/fire/PAZ061",
                   "timeZone": "America/New_York",
                   "radarStation": "KDIX"
               }
            }
            */
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[ERROR] GetForecastUrl: {ex.Message}");
            return string.Empty;
        }
    }

    public string? GetForecastUrl(string jsonString)
    {
        if (string.IsNullOrWhiteSpace(jsonString))
            return string.Empty;

        using (JsonDocument doc = JsonDocument.Parse(jsonString))
        {
            if (doc.RootElement.TryGetProperty("properties", out JsonElement props))
            {
                if (props.TryGetProperty("forecast", out JsonElement forecast))
                {
                    return forecast.GetString();
                }
            }
        }
        return string.Empty; // Or handle as an error
    }

    /// <summary>
    /// Perform any cleanup routines here.
    /// </summary>
    public void Dispose()
    {
        try { _http?.Dispose(); }
        catch (Exception ex) { Debug.WriteLine($"[ERROR] Dispose: {ex.Message}"); }
    }
}
