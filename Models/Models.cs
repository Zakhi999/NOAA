using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;

namespace NOAA
{
    #region [Zone/Grid model for api.weather.gov]
    public class WeatherForecastResponse
    {
        [JsonPropertyName("@context")]
        public object Context { get; set; }

        [JsonPropertyName("type")]
        public string Type { get; set; }

        [JsonPropertyName("geometry")]
        public Geometry Geometry { get; set; }

        [JsonPropertyName("properties")]
        public ForecastProperties Properties { get; set; }
    }

    public class Geometry
    {
        [JsonPropertyName("type")]
        public string Type { get; set; }

        // Coordinates are represented as a 3D array for Polygons: [Ring][Point][Lon/Lat]
        [JsonPropertyName("coordinates")]
        public List<List<List<double>>> Coordinates { get; set; }
    }

    public class ForecastProperties
    {
        [JsonPropertyName("units")]
        public string Units { get; set; }

        [JsonPropertyName("forecastGenerator")]
        public string ForecastGenerator { get; set; }

        [JsonPropertyName("generatedAt")]
        public DateTime GeneratedAt { get; set; } // you could use a string type here and then convert the ISO 8601 format

        [JsonPropertyName("updateTime")]
        public DateTime UpdateTime { get; set; } // you could use a string type here and then convert the ISO 8601 format

        [JsonPropertyName("validTimes")]
        public string ValidTimes { get; set; }

        [JsonPropertyName("elevation")]
        public UnitValue Elevation { get; set; }

        [JsonPropertyName("periods")]
        public List<ForecastPeriod> Periods { get; set; }
    }

    public class ForecastPeriod
    {
        [JsonPropertyName("number")]
        public int Number { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("startTime")]
        public DateTime StartTime { get; set; }

        [JsonPropertyName("endTime")]
        public DateTime EndTime { get; set; }

        [JsonPropertyName("isDaytime")]
        public bool IsDaytime { get; set; }

        [JsonPropertyName("temperature")]
        public int Temperature { get; set; }

        [JsonPropertyName("temperatureUnit")]
        public string TemperatureUnit { get; set; }

        [JsonPropertyName("temperatureTrend")]
        public string TemperatureTrend { get; set; }

        [JsonPropertyName("probabilityOfPrecipitation")]
        public UnitValue ProbabilityOfPrecipitation { get; set; }

        [JsonPropertyName("windSpeed")]
        public string WindSpeed { get; set; }

        [JsonPropertyName("windDirection")]
        public string WindDirection { get; set; }

        [JsonPropertyName("icon")]
        public string Icon { get; set; }

        [JsonPropertyName("shortForecast")]
        public string ShortForecast { get; set; }

        [JsonPropertyName("detailedForecast")]
        public string DetailedForecast { get; set; }
    }

    public class UnitValue
    {
        [JsonPropertyName("unitCode")]
        public string UnitCode { get; set; } // World Meteorological Organization code, e.g. "wmoUnit:m" ⇒ meters, "wmoUnit:percent" ⇒ %

        [JsonPropertyName("value")]
        public double? Value { get; set; } // e.g. 121.92 meters
    }
    #endregion
}
