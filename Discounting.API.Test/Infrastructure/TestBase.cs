using System.Threading.Tasks;
using Xunit;

namespace Discounting.Tests.Infrastructure
{
    public class TestBase : IClassFixture<AppState>, IAsyncLifetime
    {
        private readonly bool useNewAppStateForEachTest;

        protected TestBase(AppState appState = null)
        {
            AppState = appState;

            if (appState == null)
            {
                AppState = new AppState();
                useNewAppStateForEachTest = true;
            }
        }

        private AppState AppState { get; set; }

        public Task InitializeAsync()
        {
            return Task.CompletedTask;
        }

        public Task DisposeAsync()
        {
            if (useNewAppStateForEachTest)
            {
                AppState?.Dispose();
                AppState = null;
            }
            else
            {
                AppState.CleanupAfterTest();
            }

            return Task.CompletedTask;
        }
    }
}