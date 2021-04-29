using Discounting.Extensions;
using Xunit;

namespace Discounting.Tests.UnitTests
{
    public class NumberToRussianStringConvertorTests
    {
        [Theory]
        [InlineData(2351656.04,
            "Два миллиона триста пятьдесят одна тысяча шестьсот пятьдесят шесть рублей 04 копейки")]
        public void NumberToRussianStringTest(decimal input, string output)
        {
            var result = input.ToRussianString();
            Assert.Equal(output, result);
        }
    }
}