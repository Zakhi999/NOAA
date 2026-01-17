using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;

namespace NOAA
{
    public class ApiUrls
    {
        public string WeeklyForecast { get; set; } // https://api.weather.gov/gridpoints/PHI/34,100/forecast
        public string GridData { get; set; }       // https://api.weather.gov/gridpoints/PHI/34,100
    }

    public class MergedWeather
    {
        public DateTime Time { get; set; }
        public string ForcastPrecip { get; set; }
        public string DetailPrecip { get; set; }
    }
}

namespace NOAA_Model1
{
    #region [Zone model for api.weather.gov]
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

        /// <summary>
        /// gets populated in separate API call (grid data)
        /// </summary>
        public string PrecipitationAmount { get; set; }
    }

    public class UnitValue
    {
        [JsonPropertyName("unitCode")]
        public string UnitCode { get; set; } // World Meteorological Organization code, e.g. "wmoUnit:m" ⇒ meters, "wmoUnit:percent" ⇒ %, "wmoUnit:degC" ⇒ Celsius

        [JsonPropertyName("value")]
        public double? Value { get; set; } // e.g. 121.92 meters
    }
    #endregion

    #region [For UI]
    public class PrecipitationValue
    {
        public string Time { get; set; }
        public string Value { get; set; }
        public string UnitOfMeasure { get; set; }
    }
    #endregion
}

// Calling https://api.weather.gov/gridpoints/PHI/34,100 without the "/forecast" route returns more detailed model data.

#region [Result]
/*
{
    "@context": [
        "https://geojson.org/geojson-ld/geojson-context.jsonld",
        {
            "@version": "1.1",
            "wmoUnit": "https://codes.wmo.int/common/unit/",
            "nwsUnit": "https://api.weather.gov/ontology/unit/"
        }
    ],
    "id": "https://api.weather.gov/gridpoints/PHI/34,100",
    "type": "Feature",
    "geometry": {
        "type": "Polygon",
        "coordinates": [
            [
                [
                    -75.4999,
                    40.5179
                ],
                [
                    -75.4957,
                    40.5397
                ],
                [
                    -75.5243,
                    40.5428
                ],
                [
                    -75.5285,
                    40.5211
                ],
                [
                    -75.4999,
                    40.5179
                ]
            ]
        ]
    },
    "properties": {
        "@id": "https://api.weather.gov/gridpoints/PHI/34,100",
        "@type": "wx:Gridpoint",
        "updateTime": "2026-01-11T10:06:35+00:00",
        "validTimes": "2026-01-11T04:00:00+00:00/P7DT21H",
        "elevation": {
            "unitCode": "wmoUnit:m",
            "value": 121.92
        },
        "forecastOffice": "https://api.weather.gov/offices/PHI",
        "gridId": "PHI",
        "gridX": 34,
        "gridY": 100,
        "temperature": {
            "uom": "wmoUnit:degC",
            "values": [
                {
                    "validTime": "2026-01-11T04:00:00+00:00/PT1H",
                    "value": 3.3333333333333335
                },
                {
                    "validTime": "2026-01-11T05:00:00+00:00/PT3H",
                    "value": 2.7777777777777777
                }
            ]
        },
        "dewpoint": {
            "uom": "wmoUnit:degC",
            "values": [
                {
                    "validTime": "2026-01-11T04:00:00+00:00/PT1H",
                    "value": 2.7777777777777777
                },
                {
                    "validTime": "2026-01-11T05:00:00+00:00/PT3H",
                    "value": 2.2222222222222223
                }
            ]
        },
        "maxTemperature": {
            "uom": "wmoUnit:degC",
            "values": [
                {
                    "validTime": "2026-01-11T12:00:00+00:00/PT13H",
                    "value": 6.111111111111111
                },
                {
                    "validTime": "2026-01-18T12:00:00+00:00/PT13H",
                    "value": 1.1111111111111112
                }
            ]
        },
        "minTemperature": {
            "uom": "wmoUnit:degC",
            "values": [
                {
                    "validTime": "2026-01-11T04:00:00+00:00/PT10H",
                    "value": 2.2222222222222223
                },
                {
                    "validTime": "2026-01-18T00:00:00+00:00/PT14H",
                    "value": -6.111111111111111
                }
            ]
        },
        "relativeHumidity": {
            "uom": "wmoUnit:percent",
            "values": [
                {
                    "validTime": "2026-01-11T04:00:00+00:00/PT4H",
                    "value": 96
                },
                {
                    "validTime": "2026-01-19T00:00:00+00:00/PT1H",
                    "value": 68
                }
            ]
        },
        "apparentTemperature": {
            "uom": "wmoUnit:degC",
            "values": [
                {
                    "validTime": "2026-01-11T04:00:00+00:00/PT1H",
                    "value": 2.2222222222222223
                },
                {
                    "validTime": "2026-01-19T00:00:00+00:00/PT1H",
                    "value": -7.222222222222222
                }
            ]
        },
        "wetBulbGlobeTemperature": {
            "values": []
        },
        "heatIndex": {
            "uom": "wmoUnit:degC",
            "values": [
                {
                    "validTime": "2026-01-11T04:00:00+00:00/P7DT21H",
                    "value": null
                }
            ]
        },
        "windChill": {
            "uom": "wmoUnit:degC",
            "values": [
                {
                    "validTime": "2026-01-11T04:00:00+00:00/PT1H",
                    "value": 2.2222222222222223
                },
                {
                    "validTime": "2026-01-19T00:00:00+00:00/PT1H",
                    "value": -7.222222222222222
                }
            ]
        },
        "skyCover": {
            "uom": "wmoUnit:percent",
            "values": [
                {
                    "validTime": "2026-01-11T04:00:00+00:00/PT2H",
                    "value": 95
                },
                {
                    "validTime": "2026-01-19T00:00:00+00:00/PT1H",
                    "value": 38
                }
            ]
        },
        "windDirection": {
            "uom": "wmoUnit:degree_(angle)",
            "values": [
                {
                    "validTime": "2026-01-11T04:00:00+00:00/PT1H",
                    "value": 300
                },
                {
                    "validTime": "2026-01-18T23:00:00+00:00/PT2H",
                    "value": 290
                }
            ]
        },
        "windSpeed": {
            "uom": "wmoUnit:km_h-1",
            "values": [
                {
                    "validTime": "2026-01-11T04:00:00+00:00/PT1H",
                    "value": 5.556
                },
                {
                    "validTime": "2026-01-19T00:00:00+00:00/PT1H",
                    "value": 12.964
                }
            ]
        },
        "windGust": {
            "uom": "wmoUnit:km_h-1",
            "values": [
                {
                    "validTime": "2026-01-11T04:00:00+00:00/PT1H",
                    "value": 7.408
                },
                {
                    "validTime": "2026-01-19T00:00:00+00:00/PT1H",
                    "value": 22.224
                }
            ]
        },
        "weather": {
            "values": [
                {
                    "validTime": "2026-01-11T04:00:00+00:00/PT2H",
                    "value": [
                        {
                            "coverage": null,
                            "weather": null,
                            "intensity": null,
                            "visibility": {
                                "unitCode": "wmoUnit:km",
                                "value": null
                            },
                            "attributes": []
                        }
                    ]
                },
                {
                    "validTime": "2026-01-11T06:00:00+00:00/PT3H",
                    "value": [
                        {
                            "coverage": "patchy",
                            "weather": "fog",
                            "intensity": null,
                            "visibility": {
                                "unitCode": "wmoUnit:km",
                                "value": null
                            },
                            "attributes": []
                        }
                    ]
                },
                {
                    "validTime": "2026-01-11T09:00:00+00:00/PT9H",
                    "value": [
                        {
                            "coverage": null,
                            "weather": null,
                            "intensity": null,
                            "visibility": {
                                "unitCode": "wmoUnit:km",
                                "value": null
                            },
                            "attributes": []
                        }
                    ]
                },
                {
                    "validTime": "2026-01-11T18:00:00+00:00/PT2H",
                    "value": [
                        {
                            "coverage": "chance",
                            "weather": "rain_showers",
                            "intensity": "light",
                            "visibility": {
                                "unitCode": "wmoUnit:km",
                                "value": null
                            },
                            "attributes": []
                        },
                        {
                            "coverage": "chance",
                            "weather": "snow_showers",
                            "intensity": "light",
                            "visibility": {
                                "unitCode": "wmoUnit:km",
                                "value": null
                            },
                            "attributes": []
                        }
                    ]
                },
                {
                    "validTime": "2026-01-11T20:00:00+00:00/PT1H",
                    "value": [
                        {
                            "coverage": "chance",
                            "weather": "snow_showers",
                            "intensity": "light",
                            "visibility": {
                                "unitCode": "wmoUnit:km",
                                "value": null
                            },
                            "attributes": []
                        },
                        {
                            "coverage": "slight_chance",
                            "weather": "snow_showers",
                            "intensity": "heavy",
                            "visibility": {
                                "unitCode": "wmoUnit:km",
                                "value": null
                            },
                            "attributes": []
                        }
                    ]
                },
                {
                    "validTime": "2026-01-11T21:00:00+00:00/PT3H",
                    "value": [
                        {
                            "coverage": "slight_chance",
                            "weather": "snow_showers",
                            "intensity": "light",
                            "visibility": {
                                "unitCode": "wmoUnit:km",
                                "value": null
                            },
                            "attributes": []
                        }
                    ]
                },
                {
                    "validTime": "2026-01-12T00:00:00+00:00/P2DT12H",
                    "value": [
                        {
                            "coverage": null,
                            "weather": null,
                            "intensity": null,
                            "visibility": {
                                "unitCode": "wmoUnit:km",
                                "value": null
                            },
                            "attributes": []
                        }
                    ]
                },
                {
                    "validTime": "2026-01-14T12:00:00+00:00/PT6H",
                    "value": [
                        {
                            "coverage": "slight_chance",
                            "weather": "rain",
                            "intensity": "light",
                            "visibility": {
                                "unitCode": "wmoUnit:km",
                                "value": null
                            },
                            "attributes": []
                        }
                    ]
                },
                {
                    "validTime": "2026-01-14T18:00:00+00:00/PT18H",
                    "value": [
                        {
                            "coverage": "chance",
                            "weather": "rain",
                            "intensity": "light",
                            "visibility": {
                                "unitCode": "wmoUnit:km",
                                "value": null
                            },
                            "attributes": []
                        }
                    ]
                },
                {
                    "validTime": "2026-01-15T12:00:00+00:00/PT6H",
                    "value": [
                        {
                            "coverage": "chance",
                            "weather": "rain",
                            "intensity": "light",
                            "visibility": {
                                "unitCode": "wmoUnit:km",
                                "value": null
                            },
                            "attributes": []
                        },
                        {
                            "coverage": "slight_chance",
                            "weather": "snow",
                            "intensity": "light",
                            "visibility": {
                                "unitCode": "wmoUnit:km",
                                "value": null
                            },
                            "attributes": []
                        }
                    ]
                },
                {
                    "validTime": "2026-01-15T18:00:00+00:00/PT6H",
                    "value": [
                        {
                            "coverage": "chance",
                            "weather": "rain",
                            "intensity": "light",
                            "visibility": {
                                "unitCode": "wmoUnit:km",
                                "value": null
                            },
                            "attributes": []
                        },
                        {
                            "coverage": "chance",
                            "weather": "snow",
                            "intensity": "light",
                            "visibility": {
                                "unitCode": "wmoUnit:km",
                                "value": null
                            },
                            "attributes": []
                        }
                    ]
                },
                {
                    "validTime": "2026-01-16T00:00:00+00:00/PT6H",
                    "value": [
                        {
                            "coverage": "slight_chance",
                            "weather": "snow",
                            "intensity": "light",
                            "visibility": {
                                "unitCode": "wmoUnit:km",
                                "value": null
                            },
                            "attributes": []
                        }
                    ]
                },
                {
                    "validTime": "2026-01-16T06:00:00+00:00/P1DT12H",
                    "value": [
                        {
                            "coverage": null,
                            "weather": null,
                            "intensity": null,
                            "visibility": {
                                "unitCode": "wmoUnit:km",
                                "value": null
                            },
                            "attributes": []
                        }
                    ]
                },
                {
                    "validTime": "2026-01-17T18:00:00+00:00/PT6H",
                    "value": [
                        {
                            "coverage": "slight_chance",
                            "weather": "rain",
                            "intensity": "light",
                            "visibility": {
                                "unitCode": "wmoUnit:km",
                                "value": null
                            },
                            "attributes": []
                        },
                        {
                            "coverage": "slight_chance",
                            "weather": "snow",
                            "intensity": "light",
                            "visibility": {
                                "unitCode": "wmoUnit:km",
                                "value": null
                            },
                            "attributes": []
                        }
                    ]
                },
                {
                    "validTime": "2026-01-18T00:00:00+00:00/PT6H",
                    "value": [
                        {
                            "coverage": "slight_chance",
                            "weather": "snow",
                            "intensity": "light",
                            "visibility": {
                                "unitCode": "wmoUnit:km",
                                "value": null
                            },
                            "attributes": []
                        }
                    ]
                },
                {
                    "validTime": "2026-01-18T06:00:00+00:00/PT6H",
                    "value": [
                        {
                            "coverage": null,
                            "weather": null,
                            "intensity": null,
                            "visibility": {
                                "unitCode": "wmoUnit:km",
                                "value": null
                            },
                            "attributes": []
                        }
                    ]
                },
                {
                    "validTime": "2026-01-18T12:00:00+00:00/PT12H",
                    "value": [
                        {
                            "coverage": "chance",
                            "weather": "snow",
                            "intensity": "light",
                            "visibility": {
                                "unitCode": "wmoUnit:km",
                                "value": null
                            },
                            "attributes": []
                        }
                    ]
                }
            ]
        },
        "hazards": {
            "values": [
                {
                    "validTime": "2026-01-11T04:00:00+00:00/P1DT7H",
                    "value": []
                }
            ]
        },
        "probabilityOfPrecipitation": {
            "uom": "wmoUnit:percent",
            "values": [
                {
                    "validTime": "2026-01-11T04:00:00+00:00/PT1H",
                    "value": 5
                },
                {
                    "validTime": "2026-01-18T12:00:00+00:00/PT12H",
                    "value": 25
                }
            ]
        },
        "quantitativePrecipitation": {
            "uom": "wmoUnit:mm",
            "values": [
                {
                    "validTime": "2026-01-11T04:00:00+00:00/PT2H",
                    "value": 0.507999988645311
                },
                {
                    "validTime": "2026-01-14T06:00:00+00:00/PT6H",
                    "value": 0
                }
            ]
        },
        "iceAccumulation": {
            "uom": "wmoUnit:mm",
            "values": [
                {
                    "validTime": "2026-01-11T04:00:00+00:00/PT2H",
                    "value": 0
                },
                {
                    "validTime": "2026-01-14T06:00:00+00:00/PT6H",
                    "value": 0
                }
            ]
        },
        "snowfallAmount": {
            "uom": "wmoUnit:mm",
            "values": [
                {
                    "validTime": "2026-01-11T04:00:00+00:00/PT2H",
                    "value": 0
                },
                {
                    "validTime": "2026-01-14T06:00:00+00:00/PT6H",
                    "value": 0
                }
            ]
        },
        "snowLevel": {
            "values": []
        },
        "ceilingHeight": {
            "uom": "wmoUnit:m",
            "values": [
                {
                    "validTime": "2026-01-11T04:00:00+00:00/PT1H",
                    "value": 91.44
                },
                {
                    "validTime": "2026-01-11T20:00:00+00:00/PT17H",
                    "value": null
                }
            ]
        },
        "visibility": {
            "uom": "wmoUnit:m",
            "values": [
                {
                    "validTime": "2026-01-11T04:00:00+00:00/PT1H",
                    "value": 1448.4095616302457
                },
                {
                    "validTime": "2026-01-18T13:00:00+00:00/PT11H",
                    "value": 16093.44
                }
            ]
        },
        "transportWindSpeed": {
            "uom": "wmoUnit:km_h-1",
            "values": [
                {
                    "validTime": "2026-01-11T04:00:00+00:00/PT1H",
                    "value": 5.556
                },
                {
                    "validTime": "2026-01-19T00:00:00+00:00/PT1H",
                    "value": 18.52
                }
            ]
        },
        "transportWindDirection": {
            "uom": "wmoUnit:degree_(angle)",
            "values": [
                {
                    "validTime": "2026-01-11T04:00:00+00:00/PT1H",
                    "value": 320
                },
                {
                    "validTime": "2026-01-19T00:00:00+00:00/PT1H",
                    "value": 250
                }
            ]
        },
        "mixingHeight": {
            "uom": "wmoUnit:m",
            "values": [
                {
                    "validTime": "2026-01-11T04:00:00+00:00/PT1H",
                    "value": 176.4792
                },
                {
                    "validTime": "2026-01-19T00:00:00+00:00/PT1H",
                    "value": 143.8656
                }
            ]
        },
        "hainesIndex": {
            "values": []
        },
        "lightningActivityLevel": {
            "values": [
                {
                    "validTime": "2026-01-11T04:00:00+00:00/P2DT21H",
                    "value": 1
                }
            ]
        },
        "twentyFootWindSpeed": {
            "uom": "wmoUnit:km_h-1",
            "values": [
                {
                    "validTime": "2026-01-11T04:00:00+00:00/PT2H",
                    "value": 3.704
                },
                {
                    "validTime": "2026-01-19T00:00:00+00:00/PT1H",
                    "value": 11.112
                }
            ]
        },
        "twentyFootWindDirection": {
            "uom": "wmoUnit:degree_(angle)",
            "values": [
                {
                    "validTime": "2026-01-11T04:00:00+00:00/PT1H",
                    "value": 310
                },
                {
                    "validTime": "2026-01-18T23:00:00+00:00/PT2H",
                    "value": 290
                }
            ]
        },
        "waveHeight": {
            "uom": "wmoUnit:m",
            "values": [
                {
                    "validTime": "2026-01-11T04:00:00+00:00/P7DT18H",
                    "value": 0
                }
            ]
        },
        "wavePeriod": {
            "values": []
        },
        "waveDirection": {
            "values": []
        },
        "primarySwellHeight": {
            "values": []
        },
        "primarySwellDirection": {
            "values": []
        },
        "secondarySwellHeight": {
            "values": []
        },
        "secondarySwellDirection": {
            "values": []
        },
        "wavePeriod2": {
            "values": []
        },
        "windWaveHeight": {
            "values": []
        },
        "dispersionIndex": {
            "values": []
        },
        "pressure": {
            "values": []
        },
        "probabilityOfTropicalStormWinds": {
            "values": []
        },
        "probabilityOfHurricaneWinds": {
            "values": []
        },
        "potentialOf15mphWinds": {
            "values": []
        },
        "potentialOf25mphWinds": {
            "values": []
        },
        "potentialOf35mphWinds": {
            "values": []
        },
        "potentialOf45mphWinds": {
            "values": []
        },
        "potentialOf20mphWindGusts": {
            "values": []
        },
        "potentialOf30mphWindGusts": {
            "values": []
        },
        "potentialOf40mphWindGusts": {
            "values": []
        },
        "potentialOf50mphWindGusts": {
            "values": []
        },
        "potentialOf60mphWindGusts": {
            "values": []
        },
        "grasslandFireDangerIndex": {
            "values": []
        },
        "probabilityOfThunder": {
            "values": []
        },
        "davisStabilityIndex": {
            "values": []
        },
        "atmosphericDispersionIndex": {
            "values": [
                {
                    "validTime": "2026-01-11T04:00:00+00:00/PT1H",
                    "value": 2
                },
                {
                    "validTime": "2026-01-12T23:00:00+00:00/PT2H",
                    "value": 7
                }
            ]
        },
        "lowVisibilityOccurrenceRiskIndex": {
            "values": [
                {
                    "validTime": "2026-01-11T04:00:00+00:00/PT1H",
                    "value": 7
                },
                {
                    "validTime": "2026-01-12T23:00:00+00:00/PT2H",
                    "value": 3
                }
            ]
        },
        "stability": {
            "values": []
        },
        "redFlagThreatIndex": {
            "values": []
        }
    }
}
*/
#endregion

/// <summary>
///   Because some of the serializations shared members, I've opted to 
///   create a 2nd namespace, so we don't need one monolithic class.
/// </summary>
namespace NOAA_Model2
{
    #region [Grid model for api.weather.gov]
    public class GridpointResponse
    {
        [JsonPropertyName("@context")]
        public List<object> Context { get; set; }
        public string Id { get; set; }
        public string Type { get; set; }
        public Geometry Geometry { get; set; }
        public GridpointProperties Properties { get; set; }
    }

    public class Geometry
    {
        public string Type { get; set; }
        public List<List<List<double>>> Coordinates { get; set; }
    }

    public class GridpointProperties
    {
        [JsonPropertyName("@id")]
        public string Id { get; set; }

        [JsonPropertyName("@type")]
        public string Type { get; set; }

        public DateTime UpdateTime { get; set; }
        public string ValidTimes { get; set; }
        public UnitValue Elevation { get; set; }

        public string ForecastOffice { get; set; }
        public string GridId { get; set; }
        public int GridX { get; set; }
        public int GridY { get; set; }

        // Core meteorological fields
        public GridDataField Temperature { get; set; }
        public GridDataField Dewpoint { get; set; }
        public GridDataField MaxTemperature { get; set; }
        public GridDataField MinTemperature { get; set; }
        public GridDataField RelativeHumidity { get; set; }
        public GridDataField ApparentTemperature { get; set; }
        public GridDataField WetBulbGlobeTemperature { get; set; }
        public GridDataField HeatIndex { get; set; }
        public GridDataField WindChill { get; set; }
        public GridDataField SkyCover { get; set; }
        public GridDataField WindDirection { get; set; }
        public GridDataField WindSpeed { get; set; }
        public GridDataField WindGust { get; set; }
        public WeatherField Weather { get; set; }
        public GridDataField Hazards { get; set; }
        public GridDataField ProbabilityOfPrecipitation { get; set; }
        public GridDataField QuantitativePrecipitation { get; set; }
        public GridDataField IceAccumulation { get; set; }
        public GridDataField SnowfallAmount { get; set; }
        public GridDataField SnowLevel { get; set; }

        public GridDataField CeilingHeight { get; set; }
        public GridDataField Visibility { get; set; }

        public GridDataField TransportWindSpeed { get; set; }
        public GridDataField TransportWindDirection { get; set; }
        public GridDataField MixingHeight { get; set; }

        public GridDataField HainesIndex { get; set; }
        public GridDataField LightningActivityLevel { get; set; }

        public GridDataField TwentyFootWindSpeed { get; set; }
        public GridDataField TwentyFootWindDirection { get; set; }

        public GridDataField WaveHeight { get; set; }
        public GridDataField WavePeriod { get; set; }
        public GridDataField WaveDirection { get; set; }

        public GridDataField PrimarySwellHeight { get; set; }
        public GridDataField PrimarySwellDirection { get; set; }
        public GridDataField SecondarySwellHeight { get; set; }
        public GridDataField SecondarySwellDirection { get; set; }

        public GridDataField WavePeriod2 { get; set; }
        public GridDataField WindWaveHeight { get; set; }

        public GridDataField DispersionIndex { get; set; }
        public GridDataField Pressure { get; set; }

        public GridDataField ProbabilityOfTropicalStormWinds { get; set; }
        public GridDataField ProbabilityOfHurricaneWinds { get; set; }

        public GridDataField PotentialOf15mphWinds { get; set; }
        public GridDataField PotentialOf25mphWinds { get; set; }
        public GridDataField PotentialOf35mphWinds { get; set; }
        public GridDataField PotentialOf45mphWinds { get; set; }

        public GridDataField PotentialOf20mphWindGusts { get; set; }
        public GridDataField PotentialOf30mphWindGusts { get; set; }
        public GridDataField PotentialOf40mphWindGusts { get; set; }
        public GridDataField PotentialOf50mphWindGusts { get; set; }
        public GridDataField PotentialOf60mphWindGusts { get; set; }

        public GridDataField GrasslandFireDangerIndex { get; set; }
        public GridDataField ProbabilityOfThunder { get; set; }
        public GridDataField DavisStabilityIndex { get; set; }
        public GridDataField AtmosphericDispersionIndex { get; set; }
        public GridDataField LowVisibilityOccurrenceRiskIndex { get; set; }
        public GridDataField Stability { get; set; }
        public GridDataField RedFlagThreatIndex { get; set; }
    }

    public class UnitValue
    {
        public string UnitCode { get; set; }
        public double? Value { get; set; }
    }

    public class GridDataField
    {
        public string Uom { get; set; } // Unit of measure
        public List<GridValue> Values { get; set; }
    }

    public class GridValue
    {
        public string ValidTime { get; set; }
        public object? Value { get; set; } // nullable double type causes an exception here
    }

    public class WeatherField
    {
        public List<WeatherPeriod> Values { get; set; }
    }

    public class WeatherPeriod
    {
        public string ValidTime { get; set; }
        public List<WeatherDescriptor> Value { get; set; }
    }

    public class WeatherDescriptor
    {
        public string Coverage { get; set; }
        public string Weather { get; set; }
        public string Intensity { get; set; }
        public UnitValue Visibility { get; set; }
        public List<string> Attributes { get; set; }
    }
    #endregion
}
