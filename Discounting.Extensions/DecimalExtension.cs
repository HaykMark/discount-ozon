using System;
using RSDN;
namespace Discounting.Extensions
{
    public static class DecimalExtension
    {
        public static decimal DigitsValue(this decimal value)
        {
            return value - decimal.Truncate(value);
        }

        public static bool HasMaxDigits(this decimal value, int maxDigits)
        {
            return (value.DigitsValue() * (decimal) Math.Pow(10, maxDigits)).DigitsValue() == 0;
        }

        public static string ToRussianString(this decimal value)
        {
            return RusCurrency.Str(decimal.ToDouble(value));
        }
        
        public static string ToRussianString(this double value)
        {
            return RusCurrency.Str(value);
        }
    }
}