using System;
using System.Globalization;

namespace Baxter.Vector.Machine
{
    public static class CommonHelpers
    {
        private static readonly CultureInfo UsCulture = new CultureInfo("en-US");

        public static double atof(string s)
        {
            double d = double.Parse(s, UsCulture);
            if (double.IsNaN(d) || double.IsInfinity(d))
            {
                throw new FormatException($"'{s}' is not a valid Double value");
            }
            return (d);
        }

        public static double ToDouble(this string s)
        {
            return atof(s);
        }

        public static int atoi(String s)
        {
            return int.Parse(s, UsCulture);
        }

        public static int ToInteger(this string s)
        {
            return atoi(s);
        }
    }
}