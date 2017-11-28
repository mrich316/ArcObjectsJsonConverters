using System;
using System.Globalization;
using ArcObjectConverters;

namespace ArcObjectJsonConverters.Tests.GeoJson
{
    public static class DoubleExtensions
    {
        /// <summary>
        /// JSON.NET serializes doubles by appending ".0" if they are integers.
        /// Helper extension to ease testing.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="precision"></param>
        /// <returns></returns>
        public static string ToJsonString(this double value, int precision)
        {
            var stringValue = Math.Round(value, precision).ToString(CultureInfo.InvariantCulture);

            return stringValue.Contains(".")
                ? stringValue
                : stringValue + ".0";
        }

        public static string ToJsonString(this double value)
        {
            return value.ToJsonString(new GeoJsonSerializerSettings().Precision);
        }
    }
}
