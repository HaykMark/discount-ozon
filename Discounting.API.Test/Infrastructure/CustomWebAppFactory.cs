using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Discounting.Data.Context;
using Discounting.Seeding;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Discounting.Tests.Infrastructure
{
    /// <summary>
    ///     Creates a test web host for the system under test (SUT) and uses a test server client to handle requests and
    ///     responses to the SUT.
    /// </summary>
    public class CustomWebAppFactory<TStartup> : WebApplicationFactory<TStartup>
        where TStartup : class
    {
        private readonly Dictionary<string, string> additionalConfigs;
        private readonly SqliteConnection connection = new SqliteConnection("Data Source=:memory:");

        public CustomWebAppFactory(Dictionary<string, string> additionalConfigs = null)
        {
            this.additionalConfigs = additionalConfigs;
        }

        public bool HasPreparedDatabase { get; set; }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureAppConfiguration((builderContext, config) =>
                {
                    var env = builderContext.HostingEnvironment;

                    if (additionalConfigs != null) config.AddInMemoryCollection(additionalConfigs);
                })
                .ConfigureTestServices(services =>
                {
                    // Create a new service provider.
                    var serviceProvider = new ServiceCollection()
                        .AddEntityFrameworkSqlite()
                        .BuildServiceProvider();

                    // Add a database context (DiscountingDbContext) using an in-memory 
                    // database for testing.
                    services.AddDbContext<DiscountingDbContext>(options =>
                    {
                        options.UseSqlite(connection);
                        options.UseInternalServiceProvider(serviceProvider);
                        //options.EnableSensitiveDataLogging();
                        //options.EnableDetailedErrors();
                    });


                    // Get a reference to the database context
                    var sp = services.BuildServiceProvider();
                })
                .UseEnvironment("Testing")
                .UseStartup<TStartup>();
        }

        public async Task PrepareDatabaseAsync()
        {
            if (HasPreparedDatabase) return;

            var sp = Server.Host.Services;

            using (var scope = sp.CreateScope())
            {
                var scopedServiceProvider = scope.ServiceProvider;
                var dbContext = scopedServiceProvider.GetRequiredService<DiscountingDbContext>();

                if (connection.State == ConnectionState.Open)
                {
                    connection.Close();
                    dbContext.Database.CloseConnection();
                }

                var isDeleted = await dbContext.Database.EnsureDeletedAsync();

                // Ensure the database is created.
                await connection.OpenAsync();
                await dbContext.Database.OpenConnectionAsync();
                await dbContext.Database.EnsureCreatedAsync();

                await InitDatabaseForTestsAsync(dbContext);
            }

            HasPreparedDatabase = true;
        }

        private async Task InitDatabaseForTestsAsync(DiscountingDbContext dbContext)
        {
            var seeder = new Seeder(dbContext);
            await seeder.Seed();
        }


        // https://docs.microsoft.com/en-us/dotnet/standard/design-guidelines/dispose-pattern
        protected override void Dispose(bool disposing)
        {
            connection?.Dispose();
            base.Dispose(disposing);
        }
    }
}