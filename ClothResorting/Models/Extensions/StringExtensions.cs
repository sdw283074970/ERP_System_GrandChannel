using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;

namespace ClothResorting.Models.Extensions
{
    public static class StringExtensions
    {
        public static DateTime ConvertStringToDateTime(this string str)
        {
            var newDate = new DateTime();
            DateTime.TryParseExact(str, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out newDate);
            return newDate;
        }
    }
}