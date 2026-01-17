using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using NOAA_Model1;
using NOAA_Model2;

namespace NOAA;

public class WeatherService : IDisposable
{
    readonly HttpClient _http;
    readonly JsonSerializerOptions _options;

    public WeatherService()
    {
        _options = new JsonSerializerOptions
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
        _http = new HttpClient() { Timeout = TimeSpan.FromSeconds(30) };
        _http.DefaultRequestHeaders.Accept.Clear();
        _http.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/geo+json"));
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
            Extensions.WriteToLog($"{System.Reflection.MethodBase.GetCurrentMethod()?.Name}: {ex.Message}", LogLevel.ERROR);
            return new WeatherForecastResponse();
        }
    }

    /// <summary>
    /// Uses the Forecast Office ID "PHI" (Philadelphia, PA) by default. <br/>
    /// Other valid office IDs include: <br/>
    /// <code>
    ///   AKQ, ALY, BGM, BOX, BTV, BUF, CAE, CAR, CHS, CLE, CTP, GSP, GYX, ILM, 
    ///   ILN, LWX, MHX, OKX, PBZ, PHI, RAH, RLX, RNK, ABQ, AMA, BMX, BRO, CRP, 
    ///   EPZ, EWX, FFC, FWD, HGX, HUN, JAN, JAX, KEY, LCH, LIX, LUB, LZK, MAF, 
    ///   MEG, MFL, MLB, MOB, MRX, OHX, OUN, SHV, SJT, SJU, TAE, TBW, TSA, ABR, 
    ///   APX, ARX, BIS, BOU, CYS, DDC, DLH, DMX, DTX, DVN, EAX, FGF, FSD, GID, 
    ///   GJT, GLD, GRB, GRR, ICT, ILX, IND, IWX, JKL, LBF, LMK, LOT, LSX, MKX, 
    ///   MPX, MQT, OAX, PAH, PUB, RIW, SGF, TOP, UNR, BOI, BYZ, EKA, FGZ, GGW, 
    ///   HNX, LKN, LOX, MFR, MSO, MTR, OTX, PDT, PIH, PQR, PSR, REV, SEW, SGX, 
    ///   SLC, STO, TFX, TWC, VEF, AER, AFC, AFG, AJK, ALU, GUM, HPA, HFO, PPG, 
    ///   PQE, PQW, STU, NH1, NH2, ONA, ONP
    /// </code>
    /// </summary>
    /// <param name="gridPoint"></param>
    /// <returns></returns>
    public async Task<WeatherForecastResponse?> GetWeeklyPHIForecastAsync(string gridPoint = "34,100")
    {
        try
        {
            // https://api.weather.gov/gridpoints/PHI/34,100/forecast
            var url = $"https://api.weather.gov/gridpoints/PHI/{gridPoint}/forecast";
            var json = await _http.GetStringAsync(url);

           
            var forecast = JsonSerializer.Deserialize<WeatherForecastResponse>(json, _options);

            Debug.WriteLine($"[INFO] Forecast generated at: {forecast.Properties.GeneratedAt}");
            Debug.WriteLine($"[INFO] Today's High: {forecast.Properties.Periods[0].Temperature}°F");

            return forecast;
        }
        catch (Exception ex)
        {
            Extensions.WriteToLog($"GetWeeklyPHIForecastAsync: {ex.Message}", LogLevel.ERROR);
            return new WeatherForecastResponse();
        }
    }

    public async Task<ApiUrls?> GetForecastUrlAsync(double lat, double lon)
    {
        try
        {
            var pointUrl = $"https://api.weather.gov/points/{lat},{lon}";
            var pointJson = await _http.GetStringAsync(pointUrl);
            var url = GetForecastUrl(pointJson);
            var urlGrid = GetForecastGridDataUrl(pointJson);
            return new ApiUrls 
            { 
                WeeklyForecast = url, // used for basic weekly forecast data
                GridData = urlGrid    // used for detailed precipitation data
            };
            /* [EXAMPLE]
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
            Extensions.WriteToLog($"{System.Reflection.MethodBase.GetCurrentMethod()?.Name}: {ex.Message}", LogLevel.ERROR);
            return null;
        }
    }

    public string? GetForecastUrl(string jsonString)
    {
        if (string.IsNullOrWhiteSpace(jsonString))
            return string.Empty;

        try
        {
            using (JsonDocument doc = JsonDocument.Parse(jsonString))
            {
                // Look into the "properties" array and get the "forecast" element.
                if (doc.RootElement.TryGetProperty("properties", out JsonElement props))
                {
                    if (props.TryGetProperty("forecast", out JsonElement forecast))
                    {
                        return forecast.GetString(); // e.g. "https://api.weather.gov/gridpoints/PHI/34,100/forecast"
                    }
                }
            }
            /* [EXAMPLE]
             "properties": {
                   "@id": "https://api.weather.gov/points/40.539,-75.496",
                   "forecastOffice": "https://api.weather.gov/offices/PHI",
                   "gridId": "PHI",
                   "gridX": 34,
                   "gridY": 100,
                   "forecast": "https://api.weather.gov/gridpoints/PHI/34,100/forecast",
                   "forecastHourly": "https://api.weather.gov/gridpoints/PHI/34,100/forecast/hourly",
                   "forecastGridData": "https://api.weather.gov/gridpoints/PHI/34,100",
                   "forecastZone": "https://api.weather.gov/zones/forecast/PAZ061",
                   "county": "https://api.weather.gov/zones/county/PAC077",
                   "timeZone": "America/New_York",
                   "radarStation": "KDIX"
               }
            */
        }
        catch (Exception ex)
        {
            Extensions.WriteToLog($"{System.Reflection.MethodBase.GetCurrentMethod()?.Name}: {ex.Message}", LogLevel.ERROR);
        }
        return string.Empty;
    }

    public string? GetForecastGridDataUrl(string jsonString)
    {
        if (string.IsNullOrWhiteSpace(jsonString))
            return string.Empty;

        try
        {
            using (JsonDocument doc = JsonDocument.Parse(jsonString))
            {
                // Look into the "properties" array and get the "forecast" element.
                if (doc.RootElement.TryGetProperty("properties", out JsonElement props))
                {
                    if (props.TryGetProperty("forecastGridData", out JsonElement forecast))
                    {
                        return forecast.GetString(); // e.g. "https://api.weather.gov/gridpoints/PHI/34,100"
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Extensions.WriteToLog($"{System.Reflection.MethodBase.GetCurrentMethod()?.Name}: {ex.Message}", LogLevel.ERROR);
        }
        return string.Empty;
    }

    /// <summary>
    /// Using the data grid API will return extra details.
    /// <code>
    ///   [Arrays]
    ///    "temperature", "dewpoint", "relativeHumidity", "apparentTemperature",
    ///    "wetBulbGlobeTemperature", "heatIndex", "windChill", "skyCover",
    ///    "windDirection", "windSpeed", "windGust", "hazards", "probabilityOfPrecipitation",
    ///    "quantitativePrecipitation", "iceAccumulation", "snowfallAmount", "snowLevel"
    ///    "ceilingHeight", "visibility", "transportWindSpeed", "transportWindDirection",
    ///    "mixingHeight", "hainesIndex", "lightningActivityLevel", "twentyFootWindSpeed",
    ///    "twentyFootWindDirection", "waveHeight", "wavePeriod", "waveDirection", "primarySwellHeight", 
    ///    "primarySwellDirection", "secondarySwellHeight", "secondarySwellDirection", "wavePeriod2",
    ///    "windWaveHeight", "dispersionIndex", "pressure", "probabilityOfTropicalStormWinds",
    ///    "probabilityOfHurricaneWinds", "potentialOf15mphWinds", "potentialOf25mphWinds",
    ///    "potentialOf35mphWinds", "potentialOf45mphWinds", "potentialOf20mphWindGusts",
    ///    "potentialOf30mphWindGusts", "potentialOf40mphWindGusts", "potentialOf50mphWindGusts",
    ///    "potentialOf60mphWindGusts", "grasslandFireDangerIndex", "probabilityOfThunder",
    ///    "davisStabilityIndex", "atmosphericDispersionIndex", "lowVisibilityOccurrenceRiskIndex",
    ///    "stability", "redFlagThreatIndex"
    ///   </code>
    /// </summary>
    /// <param name="officeID">PHI is used by default</param>
    /// <param name="gridPoint">grid point inside the forecast office</param>
    /// <returns></returns>
    public async Task<List<PrecipitationValue>> GetWeeklyQuantitativePrecipitationAsync(string officeID = "PHI", string gridPoint = "34,100")
    {
        List<PrecipitationValue> values = new List<PrecipitationValue>();

        try
        {
            var url = $"https://api.weather.gov/gridpoints/{officeID}/{gridPoint}";
            var json = await _http.GetStringAsync(url);

            var forecast = JsonSerializer.Deserialize<GridpointResponse>(json, _options);

            var uom = GetUnitCode(forecast.Properties.QuantitativePrecipitation.Uom);
            Debug.WriteLine($"[INFO] QuantitativePrecipitation unit of measure is {uom}");
            double divFactor = 0;
            if (uom.Equals("mm", StringComparison.OrdinalIgnoreCase))
            {
                divFactor = 25.4; // divide the length value by 25.4 (1 millimeter = 0.0393701 inches, 1 inch = 25.4 millimeters)
            }
            foreach (var item in forecast.Properties.QuantitativePrecipitation.Values)
            {
                if (double.TryParse($"{item.Value}", out double value))
                {
                    if (!divFactor.IsInvalidOrZero())
                    {
                        values.Add(new PrecipitationValue { Time = $"{item.ValidTime}", Value = $"{value / divFactor:N1} inches", UnitOfMeasure = "inches" });
                    }
                    else
                    {
                        values.Add(new PrecipitationValue { Time = $"{item.ValidTime}", Value = $"{value / divFactor:N1} {uom}", UnitOfMeasure = $"{uom}" });
                    }
                }
                else
                {
                    Debug.WriteLine($"[WARNING] item.Value is not valid ");
                }
            }

        }
        catch (Exception ex)
        {
            Extensions.WriteToLog($"{System.Reflection.MethodBase.GetCurrentMethod()?.Name}: {ex.Message}", LogLevel.ERROR);
        }
        return values;
    }

    /// <summary>
    /// We can use the gridData url or just strip off the "/forecast" postfix.
    /// </summary>
    /// <param name="forecastUrl"></param>
    /// <returns><see cref="List{T}"/> where T is <see cref="PrecipitationValue"/></returns>
    public async Task<List<PrecipitationValue>> GetWeeklyQuantitativePrecipitationAsync(string forecastUrl)
    {
        List<PrecipitationValue> values = new List<PrecipitationValue>();

        try
        {
            // e.g. https://api.weather.gov/gridpoints/PHI/34,100
            var url = forecastUrl.Replace("/forecast","", StringComparison.OrdinalIgnoreCase);
            var json = await _http.GetStringAsync(url);

            var forecast = JsonSerializer.Deserialize<GridpointResponse>(json, _options);

            var uom = GetUnitCode(forecast.Properties.QuantitativePrecipitation.Uom);
            Debug.WriteLine($"[INFO] QuantitativePrecipitation unit of measure is {uom}");
            double divFactor = 0;
            if (uom.Equals("mm", StringComparison.OrdinalIgnoreCase))
            {
                divFactor = 25.4; // divide the length value by 25.4 (1 millimeter = 0.0393701 inches, 1 inch = 25.4 millimeters)
            }
            foreach (var item in forecast.Properties.QuantitativePrecipitation.Values)
            {
                if (double.TryParse($"{item.Value}", out double value))
                {
                    if (!divFactor.IsInvalidOrZero())
                    {
                        values.Add(new PrecipitationValue { Time = $"{item.ValidTime}", Value = $"{value / divFactor:N1} inches", UnitOfMeasure = "inches" });
                    }
                    else
                    {
                        values.Add(new PrecipitationValue { Time = $"{item.ValidTime}", Value = $"{value / divFactor:N1} {uom}", UnitOfMeasure = $"{uom}" });
                    }
                }
                else
                {
                    Debug.WriteLine($"[WARNING] item.Value is not valid ");
                }
            }

        }
        catch (Exception ex)
        {
            Extensions.WriteToLog($"{System.Reflection.MethodBase.GetCurrentMethod()?.Name}: {ex.Message}", LogLevel.ERROR);
        }
        return values;
    }

    public async Task<List<PrecipitationValue>?> GetWeeklyProbabilityOfPrecipitationAsync(string officeID = "PHI", string gridPoint = "34,100")
    {
        List<PrecipitationValue> values = new List<PrecipitationValue>();

        try
        {
            var url = $"https://api.weather.gov/gridpoints/{officeID}/{gridPoint}";
            var json = await _http.GetStringAsync(url);

            var forecast = JsonSerializer.Deserialize<GridpointResponse>(json, _options);

            var uom = GetUnitCode(forecast.Properties.ProbabilityOfPrecipitation.Uom);
            Debug.WriteLine($"[INFO] ProbabilityOfPrecipitation unit of measure is {uom}");
            foreach (var item in forecast.Properties.ProbabilityOfPrecipitation.Values)
            {
                if (double.TryParse($"{item.Value}", out double value))
                {
                    values.Add(new PrecipitationValue { Time = $"{item.ValidTime}", Value = $"{value} {(uom.StartsWith("percent", StringComparison.OrdinalIgnoreCase) ? "%" : $"{uom}")}", UnitOfMeasure = $"{uom}" });
                }
                else
                {
                    Debug.WriteLine($"[WARNING] item.Value is not valid ");
                }
            }

        }
        catch (Exception ex)
        {
            Extensions.WriteToLog($"{System.Reflection.MethodBase.GetCurrentMethod()?.Name}: {ex.Message}", LogLevel.ERROR);
        }
        return values;
    }

    #region [Helpers]
    /// <summary>
    /// Returns the unit of measure from the wmoUnit string. <br/>
    /// <code>
    ///   e.g. "wmoUnit:m" ⇒ "m"
    /// </code>
    /// </summary>
    public string GetUnitCode(string wmoUnit)
    {
        if (string.IsNullOrWhiteSpace(wmoUnit))
            return string.Empty;

        return wmoUnit.Replace("wmoUnit:", "", StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Simplified parses for the start time from a NOAA valid time string.
    /// </summary>
    /// <remarks>The method expects the input string to contain a start and end time separated by a forward
    /// slash ('/'), with each part in ISO 8601 format. Only the start time portion is parsed and returned. If the input
    /// is not in the expected format, a FormatException may be thrown.</remarks>
    /// <param name="validTime">A string representing the NOAA valid time period, formatted as "start/end" where each part is an ISO 8601 date
    /// and time.</param>
    /// <returns>A <see cref="DateTime"/> value representing the start time extracted from the valid time string.</returns>
    public DateTime ParseNoaaStartTime(string validTime)
    {
        return DateTime.Parse(validTime.Split('/')[0], null, System.Globalization.DateTimeStyles.RoundtripKind);
    }

    /// <summary>
    /// NOAA’s quirkiest formats: a start time followed by an ISO‑8601 duration.
    /// <code>
    ///   var parsed = NoaaTimeParser.ParseValidTime("2026-01-17T08:00:00+00:00/PT4H");
    ///   DateTime start = parsed.Start;       // 2026-01-17 08:00 UTC
    ///   TimeSpan duration = parsed.Duration; // 4 hours
    ///   DateTime end = parsed.End;           // 2026-01-17 12:00 UTC
    /// </code>
    /// </summary>
    /// <param name="validTime">e.g. 2026-01-17T08:00:00+00:00/PT4H</param>
    /// <returns>tuple</returns>
    /// <exception cref="FormatException"></exception>
    public (DateTime Start, TimeSpan Duration, DateTime End) ParseNoaaValidTime(string validTime)
    {
        // NOAA format: "2026-01-17T08:00:00+00:00/PT4H"
        var parts = validTime.Split('/');
        if (parts.Length != 2)
            throw new FormatException($"Invalid NOAA validTime format: {validTime}");
        // Start time
        var start = DateTime.Parse(parts[0], null, System.Globalization.DateTimeStyles.RoundtripKind);
        // Duration
        var duration = System.Xml.XmlConvert.ToTimeSpan(parts[1]);
        // End time
        var end = start + duration;
        return (start, duration, end);
    }

    /// <summary>
    /// English sentence parser to extract amount of snowfall/rainfall in inches.
    /// </summary>
    public string ExtractAmount(string sentence)
    {
        sentence = sentence.ToLower().Trim();

        // Pattern: "2 to 4 inches"
        var rangeMatch = Regex.Match(sentence, @"(\d+(\.\d+)?)\s*to\s*(\d+(\.\d+)?)\s*(inch|inches)");
        if (rangeMatch.Success)
        {
            double low = double.Parse(rangeMatch.Groups[1].Value);
            double high = double.Parse(rangeMatch.Groups[3].Value);
            return $"{low:0.0} - {high:0.0} inches";
        }
        // Pattern: "less than half an inch"
        if (sentence.Contains("less than half an inch"))
            return "< 0.5 inch";
        // Pattern: "less than an inch"
        if (sentence.Contains("less than an inch"))
            return "< 1.0 inch";
        // Pattern: "half an inch"
        if (sentence.Contains("half an inch"))
            return "0.5 inch";
        // Pattern: "around an inch"
        if (sentence.Contains("around an inch"))
            return "~ 1.0 inch";
        // Pattern: "around X inches"
        var aroundMatch = Regex.Match(sentence, @"around\s+(\d+(\.\d+)?)\s*(inch|inches)");
        if (aroundMatch.Success)
        {
            double val = double.Parse(aroundMatch.Groups[1].Value);
            return $"~ {val:0.0} inches";
        }
        // Pattern: "X inches"
        var singleMatch = Regex.Match(sentence, @"(\d+(\.\d+)?)\s*(inch|inches)");
        if (singleMatch.Success)
        {
            double val = double.Parse(singleMatch.Groups[1].Value);
            return $"{val:0.0} inches";
        }
        // No match
        return string.Empty;
    }
    #endregion

    /// <summary>
    /// Perform any cleanup routines here.
    /// </summary>
    public void Dispose()
    {
        try { _http?.Dispose(); }
        catch (Exception ex) { Debug.WriteLine($"[ERROR] {System.Reflection.MethodBase.GetCurrentMethod()?.Name}: {ex.Message}"); }
    }
}
