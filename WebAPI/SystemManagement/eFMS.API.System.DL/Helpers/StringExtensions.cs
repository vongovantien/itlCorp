using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace SystemManagement.DL.Helpers.Extensions
{
    public static class StringExtensions
    {
        private static List<string> GetValidDateTimeFormats()
        {
            var dateFormats = new[]
            {
                "dd.MM.yyyy",
                "yyyy-MM-dd",
                "yyyyMMdd",
            }.ToList();
            var timeFormats = new[]
            {
                "HH:mm:ss.fff",
                "HH:mm:ss",
                "HH:mm",
            }.ToList();

            var result = (from dateFormat in dateFormats
                          from timeFormat in timeFormats
                          select $"{dateFormat} {timeFormat}").ToList();

            return result;
        }

        public static bool ParseDateTimeEx(this string @this, CultureInfo culture, out DateTime dateTime)
        {
            if (culture == null)
            {
                culture = CultureInfo.InvariantCulture;
            }

            DateTime.TryParse(@this, culture, DateTimeStyles.None, out dateTime);
            if (dateTime.Year >= 1000 && dateTime.Year <= 9999)
                return true;
            DateTime.TryParse(@this, out dateTime);
            if (dateTime.Year >= 1000 && dateTime.Year <= 9999)
                return true;

            var dateTimeFormats = GetValidDateTimeFormats();

            DateTime.TryParseExact(@this, dateTimeFormats.ToArray(), culture, DateTimeStyles.None, out dateTime);

            if (dateTime.Year >= 1000 && dateTime.Year <= 9999)
                return true;
            return false;
        }
    }
}