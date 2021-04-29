using System;
using System.Globalization;

namespace Discounting.Extensions
{
    public static class DateTimeExtension
    {
        public static string ToRussianDateFormat(this DateTime value) =>
            value.ToString("dd.MM.yyyy");
    }
}