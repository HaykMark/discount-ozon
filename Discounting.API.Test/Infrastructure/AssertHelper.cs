using System;
using System.Net;
using System.Threading.Tasks;
using Refit;
using Xunit;

namespace Discounting.Tests.Infrastructure
{
    public static class AssertHelper
    {
        public static async Task FailOnStatusCodeAsync(Func<Task> testCode, HttpStatusCode httpStatusCode)
        {
            try
            {
                await testCode();
                Assert.True(false, "It is expected that the call fails");
            }
            catch (ApiException ex)
            {
                Assert.True(httpStatusCode == ex.StatusCode);
            }
        }

        public static Task AssertUnauthorizedAsync(Func<Task> testCode)
        {
            return FailOnStatusCodeAsync(testCode, HttpStatusCode.Unauthorized);
        }

        public static Task AssertForbiddenAsync(Func<Task> testCode)
        {
            return FailOnStatusCodeAsync(testCode, HttpStatusCode.Forbidden);
        }

        public static Task AssertNotFoundAsync(Func<Task> testCode)
        {
            return FailOnStatusCodeAsync(testCode, HttpStatusCode.NotFound);
        }

        public static Task AssertConflictAsync(Func<Task> testCode)
        {
            return FailOnStatusCodeAsync(testCode, HttpStatusCode.Conflict);
        }

        public static Task AssertUnprocessableEntityAsync(Func<Task> testCode)
        {
            return FailOnStatusCodeAsync(testCode, HttpStatusCode.UnprocessableEntity);
        }
    }
}