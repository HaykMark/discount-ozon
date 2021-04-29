using System.Net.Http;
using Discounting.Data.Context;
using Discounting.Tests.Infrastructure;
using Refit;

namespace Discounting.Tests.Fixtures.Common
{
    public partial class BaseFixture
    {
        public BaseFixture(AppState appState)
        {
            AppState = appState;
        }

        public HttpClient HttpClient => AppState.HttpClient;

        public AppState AppState { get; set; }

        public DiscountingDbContext DbContext => AppState.DbContext;

        public T GenerateClient<T>()
        {
            return RestService.For<T>(HttpClient, new RefitSettings
            {
                ContentSerializer = new JsonContentSerializer(AppState.JsonSerializerSettings)
            });
        }
    }
}