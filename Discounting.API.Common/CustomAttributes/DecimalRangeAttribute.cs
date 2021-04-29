namespace Discounting.API.Common.CustomAttributes
{
    /// <inheritdoc />
    public class DecimalRangeAttribute : CustomRangeAttribute
    {
        /// <inheritdoc />
        /// <summary>
        /// Check that the the number is non negative and maximum 18 digit (18,2)
        /// </summary>
        public DecimalRangeAttribute(double minimum = -9999999999999999.99, double maximum = 9999999999999999.99)
            : base(minimum, maximum)
        { }
    }
}