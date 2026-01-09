using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NOAA;

public enum LogLevel { DEBUG = 0, INFO = 1, WARNING = 2, ERROR = 3, SUCCESS = 4 }

public class Constants
{
    public static string MainButtonText = "Get Weather (api.weather.gov)";
    public static Uri AssetLogo = new Uri(@"assets/logo.png", UriKind.Relative);
    public static Uri AssetError = new Uri(@"assets/error.png", UriKind.Relative);
    public static Uri AssetWarning = new Uri(@"assets/warning.png", UriKind.Relative);
    public static Uri AssetNotice = new Uri(@"assets/notice.png", UriKind.Relative);
}
