using Discounting.API.Common.Filters;
using Xunit;

namespace Discounting.Tests.UnitTests
{
    public class XssValidatorTests
    {
        [Theory]
        [InlineData("*IK<mju777")]
        public void CheckDangerousString(string input)
        {
            Assert.False(CrossSiteScriptingValidation.IsDangerousString(input, out _));
        }
    }
}