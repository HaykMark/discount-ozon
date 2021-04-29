using System;
using System.Net.Http;
using System.Threading.Tasks;
using Discounting.API;
using Discounting.Common.JsonConverter;
using Discounting.Data.Context;
using Discounting.Tests.ClientApi.Account;
using Microsoft.AspNetCore.Mvc.Testing;
using Newtonsoft.Json;
using Refit;

namespace Discounting.Tests.Infrastructure
{
    /// <summary>
    ///     Represents the application state for integration testing.
    /// </summary>
    public class AppState : IDisposable
    {
        public readonly JsonSerializerSettings JsonSerializerSettings;

        private IAccountApi accountClient;

        private CustomWebAppFactory<Startup> customWebAppFactory;

        private DiscountingDbContext dbContext;

        private HttpClient httpClient;

        public AppState()
        {
            JsonSerializerSettings = new JsonSerializerSettings
            {
                Converters = new JsonConverter[] {new ClientJsonConverter()}
            };
        }

        public static string BaseUrl => "http://localhost:8081";

        private static string ApiVersion => "application/vnd.discounting-v1+json";

        public IAccountApi AccountClient
        {
            get
            {
                accountClient = RestService.For<IAccountApi>(HttpClient,
                    new RefitSettings
                    {
                        ContentSerializer = new JsonContentSerializer(JsonSerializerSettings)
                    });

                return accountClient;
            }
        }


        public HttpClient HttpClient
        {
            get
            {
                if (httpClient == null)
                {
                    httpClient = CreateHttpClient();

                    new Task(async () => await PrepareDatabaseAsync()).RunSynchronously();
                }

                return httpClient;
            }

            set => httpClient = value;
        }

        public DiscountingDbContext DbContext
        {
            get
            {
                if (dbContext == null)
                {
                    var serviceProvider = CustomWebAppFactory.Server.Host.Services;

                    dbContext = serviceProvider.GetService(typeof(DiscountingDbContext)) as DiscountingDbContext;
                }

                return dbContext;
            }
        }

        public CustomWebAppFactory<Startup> CustomWebAppFactory
        {
            get
            {
                if (customWebAppFactory == null) customWebAppFactory = new CustomWebAppFactory<Startup>();

                return customWebAppFactory;
            }

            set => customWebAppFactory = value;
        }


        public void Dispose()
        {
            //token = null;

            if (customWebAppFactory != null)
            {
                customWebAppFactory?.Dispose();
                customWebAppFactory = null;
            }

            CleanupAfterTest();
        }

        private HttpClient CreateHttpClient()
        {
            var httpClient = CustomWebAppFactory.CreateClient(new WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = false,
                BaseAddress = new Uri(BaseUrl),
                HandleCookies = true
            });
            httpClient.DefaultRequestHeaders.Add("Accept", ApiVersion);
            httpClient.DefaultRequestHeaders.Add("ipaddress", "testIpAddress");
            httpClient.DefaultRequestHeaders.Add("incident", "1");
            httpClient.BaseAddress = new Uri(BaseUrl);

            return httpClient;
        }

        public async Task PrepareDatabaseAsync()
        {
            await CustomWebAppFactory.PrepareDatabaseAsync();
        }

        public void CleanupAfterTest()
        {
            if (customWebAppFactory != null) customWebAppFactory.HasPreparedDatabase = false;

            if (httpClient != null)
            {
                httpClient?.Dispose();
                httpClient = null;
            }

            // Do not dispose DBContext as it is cached
            dbContext = null;

            accountClient = null;
        }
    }
}