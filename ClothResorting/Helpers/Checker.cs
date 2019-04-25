using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;

namespace ClothResorting.Helpers
{
    public static class Checker
    {
        public static bool CheckString(string source)
        {
            Regex regExp = new Regex("[ \f\n\r\t\v]");

            return regExp.IsMatch(source);
        }
    }
}