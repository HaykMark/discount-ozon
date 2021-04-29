using System;
using System.IO;
using System.Threading.Tasks;
using Discounting.Data.Context;
using Discounting.Entities.Account;
using Discounting.Seeding;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;

namespace Discounting.API
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", false)
                .AddEnvironmentVariables()
                .Build();
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
                .ReadFrom.Configuration(config)
                .Enrich.FromLogContext()
                .WriteTo.Seq("http://seq:5341")
                .CreateLogger();
            try
            {
                Log.Information("Starting up");
                var webHost = CreateWebHostBuilder(args).Build();

                Log.Information("Migration starting...");
                await Task.Run(async () => { await MigrateDatabaseAsync(webHost); });
                Log.Information("Migration finished");

                await webHost.RunAsync();
            }
            catch (Exception ex)
            {
                var message =
                    $"Message: {ex.Message}. InnerException: {ex.InnerException}. StackTrace: {ex.StackTrace}";
                Log.Fatal(message, "Application start-up failed");
                throw;
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }

        private static async Task MigrateDatabaseAsync(IWebHost webHost)
        {
            using var scope = webHost.Services.CreateScope();
            var services = scope.ServiceProvider;

            var context = services.GetService<DiscountingDbContext>();
            var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            Log.Information("Environment: " + environment);
            //await context.Database.MigrateAsync();
            var canConnect = await context.Database.CanConnectAsync();

            if (canConnect && !await context.Set<User>().AnyAsync())
            {
                var seeder = new Seeder(context);
                await seeder.Seed();
            }
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args)
        {
            return CreateDiscountingDefaultBuilder(args)
                .ConfigureAppConfiguration((builderContext, builder) =>
                {
                    var env = builderContext.HostingEnvironment;
                    builder.SetBasePath(env.ContentRootPath);
                })
                .UseStartup<Startup>();
        }


        /// <summary>
        /// This Builder is based on the default WebHost.CreateDefaultBuilder();`
        /// The reason we need this builder is that it prevents reloading the appsetting
        /// on change as the default builder does.
        /// 
        /// Usually reloading the appsettings is fine but with our test project, this
        /// creates a new filewatcher every time the project is build which leads to 
        /// a memory overflow.
        /// </summary>
        /// <seealso href="https://github.com/aspnet/AspNetCore/blob/v2.2.5/src/DefaultBuilder/src/WebHost.cs#L148" />
        private static IWebHostBuilder CreateDiscountingDefaultBuilder(string[] args)
        {
            // The following code is based on WebHost.CreateDefaultBuilder();
            // It does not reload appsetting on change in test environment.
            var builder = new WebHostBuilder();

            if (string.IsNullOrEmpty(builder.GetSetting(WebHostDefaults.ContentRootKey)))
            {
                builder.UseContentRoot(Directory.GetCurrentDirectory());
            }

            if (args != null)
            {
                builder.UseConfiguration(new ConfigurationBuilder().AddCommandLine(args).Build());
            }

            builder.UseKestrel((builderContext, options) =>
                {
                    options.Configure(builderContext.Configuration.GetSection("Kestrel"));
                    options.Limits.MaxRequestBodySize = 100_000_000;
                })
                .ConfigureAppConfiguration((hostingContext, config) =>
                {
                    var env = hostingContext.HostingEnvironment;

                    config
                        .AddJsonFile("appsettings.json")
                        .AddJsonFile($"appsettings.{env.EnvironmentName}.json")
                        .AddEnvironmentVariables()
                        .Build();

                    if (args != null)
                    {
                        config.AddCommandLine(args);
                    }
                })
                .UseSerilog()
                .UseDefaultServiceProvider((context, options) =>
                {
                    options.ValidateScopes = context.HostingEnvironment.IsDevelopment();
                });

            return builder;
        }
    }
}