using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Discounting.API.Common.Constants;
using Discounting.Common.Exceptions;
using Discounting.Entities.Auditing;
using Discounting.Logics;
using Discounting.Logics.Account;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using static Microsoft.AspNetCore.Http.HttpMethods;

namespace Discounting.API.Middleware
{
    public class ErrorHandlingMiddleware
    {
        private readonly RequestDelegate next;
        private readonly MvcNewtonsoftJsonOptions jsonOptions;

        public ErrorHandlingMiddleware(RequestDelegate next, IOptions<MvcNewtonsoftJsonOptions> jsonOptions)
        {
            this.next = next;
            this.jsonOptions = jsonOptions.Value;
        }

        public async Task InvokeAsync(HttpContext context, IAuditService auditService)
        {
            try
            {
                await next(context);
            }
            catch (Exception ex)
            {
                await AuditExceptionAsync(context, ex, auditService);
                await HandleExceptionAsync(context, ex);
            }
        }

        private async Task AuditExceptionAsync(HttpContext context, Exception ex, IAuditService auditService)
        {
            var url = context.Request.Path;
            if (!url.HasValue)
                return;
            var userId = context.User?.Identity?.Name;
            var sourceId = IsPut(context.Request.Method)
                ? new Regex(@"^\/api\/.+\/([A-Za-z0-9-_]+)$")
                    .Match(url.Value)
                    .Groups[1]
                    .Value
                : null;
            var message = JsonConvert.SerializeObject(ex, jsonOptions.SerializerSettings);

            context.Request.Headers.TryGetValue("ipaddress", out var ipAddress);
            context.Request.Headers.TryGetValue("incident", out var incidentValues);
            if (!string.IsNullOrEmpty(incidentValues.FirstOrDefault()) &&
                int.TryParse(incidentValues.FirstOrDefault(), out var incident))
            {
                var incidentType = (IncidentType) incident;
                switch (incidentType)
                {
                    case IncidentType.UserRegistered:
                    case IncidentType.UserAdded:
                    case IncidentType.UserEmailConfirmed:
                    case IncidentType.CompanyRegulationUploaded:
                    case IncidentType.CompanyRegulationCreated:
                    case IncidentType.CompanyRegulationSigned:
                    case IncidentType.UserBlocked:
                    case IncidentType.UserUnblocked:
                    case IncidentType.UserLoggedIn:
                    case IncidentType.UserLoggedOut:
                    case IncidentType.ContractCreated:
                    case IncidentType.ContractUpdated:
                    case IncidentType.CompanySettingsCreated:
                    case IncidentType.CompanySettingsUpdated:
                    case IncidentType.TariffCreated:
                    case IncidentType.TemplatedAddedToBuyer:
                    case IncidentType.DiscountRegistryDeclined:
                    case IncidentType.TemplateForBuyerUpdated:
                    case IncidentType.SuppliesAdded:
                    case IncidentType.SuppliesVerifiedSeller:
                    case IncidentType.SuppliesVerifiedBuyer:
                    case IncidentType.DiscountCreated:
                    case IncidentType.RegistryCreated:
                    case IncidentType.RegistrySigned:
                    case IncidentType.DiscountRegistryConfirmed:
                    case IncidentType.DiscountConfirmedPercentageChanged:
                    case IncidentType.RegistryUpdated:
                    case IncidentType.DiscountRegistryUpdated:
                    case IncidentType.RegistryDeclined:
                    case IncidentType.UFDocumentCreated:
                    case IncidentType.UFDocumentSigned:
                    case IncidentType.UFDocumentDeclined:
                    case IncidentType.CompanyRegulationUpdated:
                    case IncidentType.NewUserRegulationCreated:
                    case IncidentType.NewUserRegulationUploaded:
                    case IncidentType.NewUserRegulationSigned:
                    case IncidentType.PasswordChanged:
                    case IncidentType.PasswordReset:
                    case IncidentType.RegistrySignatureVerification:
                    case IncidentType.UFDocumentSignatureVerification:
                    case IncidentType.CompanyRegulationSignatureVerification:
                    case IncidentType.UserRegulationSignatureVerification:
                        try
                        {
                            await auditService.CreateAsync(new Audit
                            {
                                Incident = incidentType,
                                Message = message,
                                UserId = string.IsNullOrEmpty(userId) ? (Guid?) null : Guid.Parse(userId),
                                SourceId = sourceId,
                                IncidentDate = DateTime.UtcNow,
                                IncidentResult = IncidentResult.Failed,
                                IpAddress = ipAddress.FirstOrDefault()
                            });
                        }
                        catch (Exception)
                        {
                            throw ex;
                        }

                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception ex)
        {
            var response = context.Response;

            if (!(ex is IHttpException))
            {
                // Hide exception message in production
                var env =
                    context.RequestServices.GetService(typeof(IWebHostEnvironment)) as IWebHostEnvironment;
                var message = env.IsDevelopment()
                    ? $"{ex.Message} {ex.InnerException?.Message ?? string.Empty}"
                    : "";
                ex = new UnknownException(message, ex);
            }

            response.StatusCode = ex is IHttpException httpEx
                ? httpEx.HttpStatus
                : Discounting.Common.Types.StatusCodes.Status500InternalServerError;
            response.ContentType = "application/json";

            var result = JsonConvert.SerializeObject(ex, jsonOptions.SerializerSettings);
            await response.WriteAsync(result);

            if (ex is UnknownException)
            {
                throw ex; // rethrow to make it appear on console output
            }
        }
    }
}