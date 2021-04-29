using System;
using System.IO;
using Discounting.Extensions;
using Microsoft.AspNetCore.Hosting;


namespace Discounting.API.Common.Email
{
    public static class EmailTemplates
    {
        public static string GetConfirmationHtmlString(IWebHostEnvironment environment, string firstName,
            string lastName, string callBackUrl)
        {
            //Get TemplateFile located at wwwroot/Templates/EmailTemplate/
            var pathToFile = Path.Combine(environment.WebRootPath, "Templates", "EmailTemplate",
                "ConfirmAccountRegistration.html");
            var htmlString = File.ReadAllText(pathToFile);

            return htmlString
                .Replace("{{Username}}", $"{firstName} {lastName}")
                .Replace("{{Link}}", callBackUrl);
        }

        public static string GetContractHtmlString(IWebHostEnvironment environment, string buyerName,
            ContractEmailEventType type, string callBackUrl)
        {
            var pathToFile = Path.Combine(environment.WebRootPath, "Templates", "EmailTemplate",
                $"Contract{type.ToString()}.html");
            var htmlString = File.ReadAllText(pathToFile);

            return htmlString
                .Replace("{{BuyerName}}", $"{buyerName}")
                .Replace("{{Link}}", callBackUrl);
        }

        public static string GetUnformalizedDocumentHtmlString(
            IWebHostEnvironment environment,
            string companyShortName,
            string topic,
            UnformalizedDocumentEmailEventType type,
            string callBackUrl
        )
        {
            var pathToFile = Path.Combine(environment.WebRootPath, "Templates", "EmailTemplate",
                $"UnformalizedDocument{type.ToString()}.html");
            var htmlString = File.ReadAllText(pathToFile);

            return htmlString
                .Replace("{{Name}}", $"{companyShortName}")
                .Replace("{{Subject}}", $"{topic}")
                .Replace("{{Link}}", callBackUrl);
        }

        public static string GetRegistryHtmlString(
            IWebHostEnvironment environment,
            string companyShortName,
            string role,
            int no,
            DateTime date,
            RegistryEmailEventType type,
            string callBackUrl
        )
        {
            var pathToFile = Path.Combine(environment.WebRootPath, "Templates", "EmailTemplate",
                $"Registry{type.ToString()}.html");
            var htmlString = File.ReadAllText(pathToFile);

            return htmlString
                .Replace("{{Name}}", $"{companyShortName}")
                .Replace("{{No}}", $"{no}")
                .Replace("{{Date}}", $"{date.ToRussianDateFormat()}")
                .Replace("{{Role}}", $"{role}")
                .Replace("{{Link}}", callBackUrl);
        }

        public static string GetCompanyHtmlString(
            IWebHostEnvironment environment,
            string companyShortName,
            string remark,
            CompanyEmailEventType type
        )
        {
            var pathToFile = Path.Combine(environment.WebRootPath, "Templates", "EmailTemplate",
                $"Company{type.ToString()}.html");
            var htmlString = File.ReadAllText(pathToFile)
                .Replace("{{Name}}", $"{companyShortName}");

            return type == CompanyEmailEventType.Deactivation
                ? htmlString.Replace("{{Reason}}", $"{remark}")
                : htmlString;
        }

        public static string GetUserHtmlString(
            IWebHostEnvironment environment,
            string userName,
            string remark,
            UserEmailEventType type
        )
        {
            var pathToFile = Path.Combine(environment.WebRootPath, "Templates", "EmailTemplate",
                $"User{type.ToString()}.html");
            var htmlString = File.ReadAllText(pathToFile)
                .Replace("{{Name}}", $"{userName}");

            return type == UserEmailEventType.Deactivation
                ? htmlString.Replace("{{Reason}}", $"{remark}")
                : htmlString;
        }

        public static string GetFactoringAgreementHtmlString(
            IWebHostEnvironment environment,
            string companyName,
            FactoringAgreementEmailEventType type,
            string callBackUrl
        )
        {
            var pathToFile = Path.Combine(environment.WebRootPath, "Templates", "EmailTemplate",
                $"{type.ToString()}.html");
            var htmlString = File.ReadAllText(pathToFile);

            return htmlString
                .Replace("{{CompanyName}}", $"{companyName}")
                .Replace("{{Link}}", callBackUrl);
        }
    }
}