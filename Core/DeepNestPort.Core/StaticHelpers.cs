using System.Globalization;

namespace DeepNestPort.Core
{
    public static class StaticHelpers
    {
        public static double ToDouble(this string str)
        {
            return double.Parse(str.Replace(",", "."), CultureInfo.InvariantCulture);
        }
    }
}