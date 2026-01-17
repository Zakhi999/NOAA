using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NOAA
{
    /// <summary>
    /// Example usage:
    /// <code>
    ///   UnitConverter.MmToInches(12.7); ⇒ 0.5
    ///   UnitConverter.MmToInchesFormatted(12.7); ⇒ "0.5 inch"
    ///   UnitConverter.InchesRangeFormatted(2, 4); ⇒ "2.0 - 4.0 inches"
    ///   UnitConverter.LessThanInchesFormatted(0.5); ⇒ "< 0.5 inch"
    /// </code>
    /// </summary>
    public static class UnitConverter
    {
        const double MmPerInch = 25.4;
        public static double MmToInches(double mm) => mm / MmPerInch;
        public static double InchesToMm(double inches) => inches * MmPerInch;
        public static double MetersToFeet(double? meters) => (meters ?? 0) * 3.28084;
        public static string MmToInchesFormatted(double mm, int decimals = 2) => $"{Math.Round(MmToInches(mm), decimals)} inch";
        public static string InchesRangeFormatted(double low, double high, int decimals = 1) => $"{Math.Round(low, decimals)} - {Math.Round(high, decimals)} inches";
        public static string LessThanInchesFormatted(double inches, int decimals = 1) => $"< {Math.Round(inches, decimals)} inch";
    }

}
