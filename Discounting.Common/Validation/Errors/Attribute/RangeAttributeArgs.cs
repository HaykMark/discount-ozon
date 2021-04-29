using System;

namespace Discounting.Common.Validation.Errors.Attribute
{
    public class RangeAttributeArgs<T> where T : IComparable
    {
        public T Min { get; set; }
        public T Max { get; set; }
    }
}