using System;
using System.Globalization;

namespace Sirius.Shared.Helper
{
    public static class DateTimeHelper
    {
        public static DateTime StringToDateTime(this string date)
        {
            return  DateTime.ParseExact(date, "yyyyMMdd",
                CultureInfo.InvariantCulture);
        }
        
        public static DateTime? StringToNullableDateTime(this string date)
        {
            if (string.IsNullOrWhiteSpace(date))
            {
                return null;
            }
            
            return  DateTime.ParseExact(date, "yyyyMMdd",
                CultureInfo.InvariantCulture);
        }

        public static string DateTimeToString(this DateTime date)
        {
            return date.ToString("yyyyMMdd");
        }
    }
}