using System;
using Discounting.Entities.TariffDiscounting;

namespace Discounting.Entities.Extensions
{
    public static class DaysOfWeekExtension
    {
        public static DaysOfWeek ToFlag(this DayOfWeek dayOfWeek)
        {
            var mask = 1 << (int)dayOfWeek;
            return (DaysOfWeek)Enum.ToObject(typeof(DaysOfWeek), mask);
        }
    }
}