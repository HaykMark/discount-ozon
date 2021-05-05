#nullable enable
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using AutoMapper;
using Discounting.API.Common.Constants;
using Discounting.API.Common.CustomBindings.QueryStringBindings;
using Discounting.API.Common.Extensions;
using Discounting.API.Common.Filters;
using Discounting.API.Common.Mapping;
using Discounting.API.Common.Options;
using Discounting.API.Middleware;
using Discounting.Data.Context;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Discounting.Common.JsonConverter;
using Discounting.Common.Options;
using Discounting.Logics;
using Microsoft.OpenApi.Models;
using Discounting.Logics.Account;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using Serilog;
using Swashbuckle.AspNetCore.SwaggerUI;
using SwaggerOptions = Discounting.API.Common.Options.SwaggerOptions;

namespace Discounting.API
{
    public class Startup
    {
        public Startup(IConfiguration configuration, IWebHostEnvironment env)
        {
            Configuration = configuration;
            HostingEnvironment = env;
        }

        public IConfiguration Configuration { get; }
        public IWebHostEnvironment HostingEnvironment { get; }

        private readonly string uploadsPath = Path.Combine(Directory.GetCurrentDirectory(), "Uploads");

        private bool IsTestEnvironment => HostingEnvironment.IsEnvironment("Testing");

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<BearerTokensOptions>(Configuration.GetSection("BearerTokens"));
            services.Configure<ApiSettingsOptions>(Configuration.GetSection("ApiSettings"));
            services.Configure<BearerTokensOptions>(Configuration.GetSection("UploadOptions"));
            services.Configure<SmtpSettingsOptions>(Configuration.GetSection("SmtpSettings"));

            services.Configure<FormOptions>(options =>
            {
                options.MultipartBodyLengthLimit = 1024 * 1024 * 100; // 100MB
            });

            services.Configure<UploadOptions>(options =>
            {
                Configuration.GetSection("UploadOptions").Bind(options);
                options.Path = uploadsPath;
            });

            var httpContextAccessor = new HttpContextAccessor();
            services.AddSingleton<IHttpContextAccessor>(httpContextAccessor);
            services.AddAutoMapper(expr => expr.AddProfile(new MappingProfile()), typeof(Startup));

            if (!IsTestEnvironment)
            {
                services.AddDbContext<DiscountingDbContext>(options =>
                    options.UseNpgsql(Configuration.GetConnectionString("DefaultConnection")));
                services.AddCors(options =>
                {
                    var origins = Configuration.GetSection("AllowedOriginsList")
                        .AsEnumerable()
                        .Select(x => x.Value)
                        .Where(x => x != null)
                        .ToArray();
                    var methods = Configuration.GetSection("AllowedMethodsList")
                        .AsEnumerable()
                        .Select(x => x.Value)
                        .Where(x => x != null)
                        .ToArray();

                    options.AddPolicy("CorsPolicy",
                        builder => builder
                            .WithOrigins(origins)
                            .WithMethods(methods)
                            .AllowAnyHeader()
                            .AllowCredentials()
                            .WithExposedHeaders("content-disposition")
                            .SetPreflightMaxAge(TimeSpan.FromSeconds(2520)));
                });
            }

            services.AddCustomServices();
            services.AddCustomAuthentication(Configuration);
            services.AddAntiforgery(x => x.HeaderName = "X-XSRF-TOKEN");
            services.AddControllers(options =>
                {
                    options.Filters.Add(typeof(ResponsePostProcessor));
                    options.Filters.Add(typeof(FirewallActionFilter));
                    options.Filters.Add(typeof(ValidateModelFilter));
                    options.Filters.Add(typeof(XssValidatorActionFilter));
                    options.Filters.Add(typeof(RouteValidatorActionFilter));
                    options.AllowEmptyInputInBodyModelBinding = true;
                    options.Conventions.Add(new SparseQueryParamConvention());
                })
                .AddNewtonsoftJson(options =>
                {
                    var serializerSettings = JsonSerializationSettingsProvider.GetSerializeSettings();
                    options.SerializerSettings.TypeNameHandling = serializerSettings.TypeNameHandling;
                    options.SerializerSettings.Formatting = serializerSettings.Formatting;
                    options.SerializerSettings.Converters = serializerSettings.Converters;
                    options.SerializerSettings.ContractResolver = serializerSettings.ContractResolver;
                    options.SerializerSettings.DateFormatHandling = serializerSettings.DateFormatHandling;
                    options.SerializerSettings.DateTimeZoneHandling = serializerSettings.DateTimeZoneHandling;
                    options.SerializerSettings.DateParseHandling = serializerSettings.DateParseHandling;
                    options.SerializerSettings.ObjectCreationHandling = serializerSettings.ObjectCreationHandling;
                    options.SerializerSettings.ReferenceLoopHandling = serializerSettings.ReferenceLoopHandling;
                    options.SerializerSettings.MissingMemberHandling = serializerSettings.MissingMemberHandling;
                    options.SerializerSettings.NullValueHandling = serializerSettings.NullValueHandling;
                    options.SerializerSettings.PreserveReferencesHandling =
                        serializerSettings.PreserveReferencesHandling;
                    options.SerializerSettings.Converters.Add(new UrlJsonConverter(httpContextAccessor));
                })
                .SetCompatibilityVersion(CompatibilityVersion.Version_3_0);

            if (!IsTestEnvironment)
            {
                var swaggerOptions = new SwaggerOptions();
                services.AddSwaggerGen(c =>
                {
                    c.SwaggerDoc("v1",
                        new OpenApiInfo
                        {
                            Title = swaggerOptions.Title,
                            Version = swaggerOptions.Version,
                            Description = swaggerOptions.Description
                        });
                    c.AddSecurityDefinition(
                        "Bearer",
                        new OpenApiSecurityScheme
                        {
                            In = ParameterLocation.Header,
                            Description = "Please enter into field the word 'Bearer' following by space and JWT",
                            Name = "Authorization",
                            Type = SecuritySchemeType.ApiKey
                        });
                    c.AddSecurityRequirement(new OpenApiSecurityRequirement
                    {
                        {
                            new OpenApiSecurityScheme
                            {
                                Reference = new OpenApiReference {Type = ReferenceType.SecurityScheme, Id = "Bearer"}
                            },
                            new List<string>()
                        }
                    });
                });
            }

            services.AddApiVersioning(o => o.ApiVersionReader = new MediaTypeApiVersionReader());
            services.AddHealthChecks();
            services.AddHttpClient<SignatureVerifierService>(c =>
            {
                c.BaseAddress = new Uri(Configuration.GetSection("ApiSettings")["SignatureVerifierUrl"]);
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILogger<Startup> logger)
        {
            app.UseForwardedHeaders(new ForwardedHeadersOptions
            {
                ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
            });
            app.UseRouting();
            app.UseCors("CorsPolicy");
            app.UseMiddleware(typeof(ErrorHandlingMiddleware));
            app.UseMiddleware<LogEnrichmentMiddleware>();

            app.UseAuthentication();
            app.UseAuthorization();
            app.UseStaticFiles();
            app.UseStaticFiles(new StaticFileOptions
            {
                FileProvider = new PhysicalFileProvider(uploadsPath),
                RequestPath = $"/{Routes.Uploads}"
            });

            app.UseSerilogRequestLogging();

            app.UseAuthorization();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapHealthChecks("/api/ping", new HealthCheckOptions()
                {
                    ResponseWriter = null,
                    ResultStatusCodes =
                    {
                        [HealthStatus.Healthy] = StatusCodes.Status204NoContent,
                        [HealthStatus.Degraded] = StatusCodes.Status204NoContent,
                        [HealthStatus.Unhealthy] = StatusCodes.Status503ServiceUnavailable
                    }
                });
                endpoints.MapGet("/", async context =>
                {
                    context.Response.ContentType = "text/plain";

                    // Host info
                    var name = Dns.GetHostName(); // get container id
                    var ip = (await Dns.GetHostEntryAsync(name)).AddressList.FirstOrDefault(x =>
                        x.AddressFamily == AddressFamily.InterNetwork);
                    Console.WriteLine($"Host Name: {Environment.MachineName} \t {name}\t {ip}");
                    await context.Response.WriteAsync($"Host Name: {Environment.MachineName}{Environment.NewLine}");
                    await context.Response.WriteAsync(Environment.NewLine);

                    // Request method, scheme, and path
                    await context.Response.WriteAsync($"Request Method: {context.Request.Method}{Environment.NewLine}");
                    await context.Response.WriteAsync($"Request Scheme: {context.Request.Scheme}{Environment.NewLine}");
                    await context.Response.WriteAsync(
                        $"Request URL: {context.Request.GetDisplayUrl()}{Environment.NewLine}");
                    await context.Response.WriteAsync($"Request Path: {context.Request.Path}{Environment.NewLine}");

                    // Headers
                    await context.Response.WriteAsync($"Request Headers:{Environment.NewLine}");
                    foreach (var (key, value) in context.Request.Headers)
                    {
                        await context.Response.WriteAsync($"\t {key}: {value}{Environment.NewLine}");
                    }

                    await context.Response.WriteAsync(Environment.NewLine);

                    // Connection: RemoteIp
                    await context.Response.WriteAsync($"Request Remote IP: {context.Connection.RemoteIpAddress}");
                });
            });
            if (!IsTestEnvironment)
            {
                app.UseSwagger();
                app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Discounting API V1");
                    c.DefaultModelsExpandDepth(1); // model shown in list at the bottom
                    c.DefaultModelExpandDepth(1); // model shown in endpoint
                    c.DefaultModelRendering(ModelRendering.Model);
                    c.DocExpansion(DocExpansion.List);
                    c.EnableDeepLinking();
                    c.EnableFilter();
                });
            }
        }

        // private string GetConnectionString()
        // {
        //     var connectionString = Configuration.GetConnectionString("DefaultConnection");
        //     var variables = Environment.GetEnvironmentVariables();
        //     if (connectionString != null)
        //     {
        //         var keys =variables.Keys.Cast<object?>().Where(key => key != null).ToArray();
        //         Log.Information($"Keys: {string.Join(",", keys)}");
        //
        //         var replacedConnectionString = variables.Keys.Cast<object?>()
        //             .Where(key => key != null && variables[key] != null && key.ToString()!.StartsWith("DB_"))
        //             .Aggregate(connectionString, (current, key) =>
        //                 current.Replace("$" + key, variables[key ?? ""]?.ToString()));
        //         Log.Information($"DefaultConnection: {replacedConnectionString}");
        //         return replacedConnectionString;
        //     }
        //
        //     throw new AggregateException("No connection string was specified");
        // }
    }
}