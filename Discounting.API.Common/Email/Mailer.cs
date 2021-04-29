using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discounting.API.Common.Options;
using Discounting.Common.Exceptions;
using Discounting.Entities;
using Discounting.Entities.Account;
using Discounting.Entities.CompanyAggregates;
using Discounting.Extensions;
using MailKit;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;

namespace Discounting.API.Common.Email
{
    public interface IMailer
    {
        Task SendUnformalizedDocumentEmailAsync(
            UnformalizedDocument contract,
            Company company,
            UnformalizedDocumentEmailEventType type,
            string returnUrl
        );

        Task SendRegistryEmailAsync(
            Registry registry,
            Company company,
            RegistryEmailEventType type,
            string returnUrl
        );

        Task SendCompanyEmailAsync(
            Company company,
            CompanyEmailEventType type
        );

        Task SendUserEmailAsync(
            User user,
            UserEmailEventType type
        );

        Task SendFactoringAgreementEmailAsync(
            FactoringAgreement factoringAgreement,
            FactoringAgreementEmailEventType type,
            string returnUrl
        );

        Task SendEmailAsync(IEnumerable<string> emails, string subject, string body);
    }

    public class Mailer : IMailer
    {
        private readonly SmtpSettingsOptions options;
        private readonly IWebHostEnvironment environment;
        private readonly ILogger<Mailer> logger;

        public Mailer(
            IOptions<SmtpSettingsOptions> options,
            IWebHostEnvironment environment,
            ILogger<Mailer> logger
        )
        {
            this.environment = environment;
            this.logger = logger;
            this.options = options.Value;
        }

        public async Task SendUnformalizedDocumentEmailAsync(
            UnformalizedDocument contract,
            Company company,
            UnformalizedDocumentEmailEventType type,
            string returnUrl
        )
        {
            var html = EmailTemplates.GetUnformalizedDocumentHtmlString(
                environment,
                company.ShortName,
                contract.Topic,
                type,
                returnUrl
            );
            var subject = type switch
            {
                UnformalizedDocumentEmailEventType.Signed => $"{company.ShortName} подписал сообщение",
                UnformalizedDocumentEmailEventType.Sent => $"{company.ShortName} прислал новое сообщение",
                UnformalizedDocumentEmailEventType.Declined => $"{company.ShortName} отклонил сообщение",
                _ => throw new ArgumentOutOfRangeException()
            };
            var emails = new List<string>();
            if (type != UnformalizedDocumentEmailEventType.Sent)
            {
                emails.AddRange(contract.Sender.Users.Select(u => u.Email));
            }

            emails.AddRange(contract.Receivers
                .Where(r => r.ReceiverId != company.Id)
                .SelectMany(r => r.Receiver.Users.Select(u => u.Email)));

            await SendEmailAsync(emails, subject, html);
        }

        public async Task SendFactoringAgreementEmailAsync(
            FactoringAgreement factoringAgreement,
            FactoringAgreementEmailEventType type,
            string returnUrl
        )
        {
            string subject, companyName;
            var emails = new List<string>();
            switch (type)
            {
                case FactoringAgreementEmailEventType.FactoringAgreementAdded:
                    companyName = factoringAgreement.Company.ShortName;
                    subject = $"{companyName} добавил договор факторинга";
                    emails.AddRange(factoringAgreement.Bank.Users.Select(u => u.Email));
                    break;
                case FactoringAgreementEmailEventType.FactoringAgreementSupplyAdded:
                    companyName = factoringAgreement.Company.ShortName;
                    subject = $"{companyName} добавил договор поставки";
                    emails.AddRange(factoringAgreement.Bank.Users.Select(u => u.Email));
                    break;
                case FactoringAgreementEmailEventType.FactoringAgreementSupplyConfirmed:
                    companyName = factoringAgreement.Bank.ShortName;
                    subject = $"{companyName} подтвердил договор поставки";
                    emails.AddRange(factoringAgreement.Company.Users.Select(u => u.Email));
                    break;
                case FactoringAgreementEmailEventType.FactoringAgreementSupplyDeactivated:
                    companyName = factoringAgreement.Bank.ShortName;
                    subject = $"{companyName} заблокировал договор поставки";
                    emails.AddRange(factoringAgreement.Company.Users.Select(u => u.Email));
                    break;
                case FactoringAgreementEmailEventType.FactoringAgreementSupplyActivated:
                    companyName = factoringAgreement.Bank.ShortName;
                    subject = $"{companyName} разблокировал договор поставки";
                    emails.AddRange(factoringAgreement.Company.Users.Select(u => u.Email));
                    break;
                default:
                    throw new ForbiddenException();
            }

            var html = EmailTemplates.GetFactoringAgreementHtmlString(
                environment,
                companyName,
                type,
                returnUrl
            );

            await SendEmailAsync(emails, subject, html);
        }

        public async Task SendRegistryEmailAsync(
            Registry registry,
            Company company,
            RegistryEmailEventType type,
            string returnUrl
        )
        {
            var role = "";
            if (type == RegistryEmailEventType.Signed)
            {
                if (registry.Contract.SellerId == company.Id)
                {
                    role = "Поставщик";
                }
                else if (registry.Contract.BuyerId == company.Id)
                {
                    role = "Покупатель";
                }
                else
                {
                    role = "Фактор";
                }
            }

            var html = EmailTemplates.GetRegistryHtmlString(
                environment,
                company.ShortName,
                role,
                registry.Number,
                registry.Date,
                type,
                returnUrl
            );

            var subject = type switch
            {
                RegistryEmailEventType.Declined =>
                    $"{company.ShortName} отклонил реестр № {registry.Number} от {registry.Date.ToRussianDateFormat()}",
                RegistryEmailEventType.BuyerConfirmedDiscount =>
                    $"{company.ShortName} согласовал реестр № {registry.Number} от {registry.Date.ToRussianDateFormat()}",
                RegistryEmailEventType.SellerSignedDiscount =>
                    $"{company.ShortName} отправил на подтверждение и подписание реестр № {registry.Number} от {registry.Date.ToRussianDateFormat()}",
                RegistryEmailEventType.Signed =>
                    $"{company.ShortName} подписал реестр № {registry.Number} от {registry.Date.ToRussianDateFormat()}",
                RegistryEmailEventType.BuyerSignedDiscount => 
                    $"{company.ShortName} подписал реестр № {registry.Number} от {registry.Date.ToRussianDateFormat()}",
                RegistryEmailEventType.BuyerConfirmedDiscountWithPercentageChange => 
                    $"{company.ShortName} согласовал реестр № {registry.Number} от {registry.Date.ToRussianDateFormat()}  с редактированием % дисконта",
                _ => throw new ArgumentOutOfRangeException()
            };
            var emails = new List<string>();
            switch (type)
            {
                case RegistryEmailEventType.Declined:
                    //Seller declined: Send email to Buyers
                    if (registry.SignStatus != RegistrySignStatus.NotSigned && company.Id == registry.Contract.SellerId)
                    {
                        emails.AddRange(registry.Contract.Buyer.Users.Select(u => u.Email));
                    }
                    //Buyer declined: Send email to Sellers
                    else if (company.Id == registry.Contract.BuyerId)
                    {
                        emails.AddRange(registry.Contract.Seller.Users.Select(u => u.Email));
                    }
                    //Bank declined: Send email to Sellers and Buyers
                    else if(registry.SignStatus == RegistrySignStatus.SignedBySellerBuyer)
                    {
                        emails.AddRange(registry.Contract.Buyer.Users.Select(u => u.Email));
                        emails.AddRange(registry.Contract.Seller.Users.Select(u => u.Email));
                    }
                   
                    break;
                case RegistryEmailEventType.SellerSignedDiscount:
                    emails.AddRange(registry.Contract.Buyer.Users.Select(u => u.Email));
                    break;
                case RegistryEmailEventType.BuyerSignedDiscount:
                    emails.AddRange(registry.Contract.Seller.Users.Select(u => u.Email));
                    break;
                case RegistryEmailEventType.BuyerConfirmedDiscountWithPercentageChange:
                    emails.AddRange(registry.Contract.Seller.Users.Select(u => u.Email));
                    break;
                case RegistryEmailEventType.BuyerConfirmedDiscount:
                    emails.AddRange(registry.Contract.Seller.Users.Select(u => u.Email));
                    break;
                case RegistryEmailEventType.Signed:
                    switch (registry.SignStatus)
                    {
                        case RegistrySignStatus.SignedBySeller:
                            emails.AddRange(registry.Contract.Buyer.Users.Select(u => u.Email));
                            break;
                        case RegistrySignStatus.SignedByBuyer:
                            emails.AddRange(registry.Contract.Seller.Users.Select(u => u.Email));
                            break;
                        case RegistrySignStatus.SignedBySellerBuyer:
                            emails.AddRange(registry.Contract.Seller.Users.Select(u => u.Email));
                            emails.AddRange(registry.Bank.Users.Select(u => u.Email));
                            break;
                        case RegistrySignStatus.SignedByAll:
                            emails.AddRange(registry.Contract.Seller.Users.Select(u => u.Email));
                            emails.AddRange(registry.Contract.Buyer.Users.Select(u => u.Email));
                            break;
                    }

                    break;
            }

            await SendEmailAsync(emails, subject, html);
        }

        public async Task SendCompanyEmailAsync(
            Company company,
            CompanyEmailEventType type
        )
        {
            var html = EmailTemplates.GetCompanyHtmlString(
                environment,
                company.ShortName,
                company.DeactivationReason,
                type
            );

            var emails = new List<string>();
            emails.AddRange(company.Users
                .Where(u => u.IsActive && u.IsEmailConfirmed)
                .Select(u => u.Email));
            var subject = type switch
            {
                CompanyEmailEventType.Deactivation => "Блокировка организации",
                CompanyEmailEventType.Activation => "Разблокировка организации",
                _ => throw new ArgumentOutOfRangeException()
            };
            await SendEmailAsync(emails, subject, html);
        }

        public async Task SendUserEmailAsync(
            User user,
            UserEmailEventType type
        )
        {
            var html = EmailTemplates.GetUserHtmlString(
                environment,
                user.Email,
                user.DeactivationReason,
                type
            );

            var subject = type switch
            {
                UserEmailEventType.Deactivation => "Блокировка пользователя",
                UserEmailEventType.Activation => "Разблокировка пользователя",
                _ => throw new ArgumentOutOfRangeException()
            };
            await SendEmailAsync(new List<string> {user.Email}, subject, html);
        }

        public async Task SendEmailAsync(IEnumerable<string> emails, string subject, string body)
        {
            logger.LogInformation($"Start sending email. Subject: {subject}");
            var emailAddresses = emails as string[] ?? emails.ToArray();
            if (!emailAddresses.Any())
            {
                return;
            }

            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(options.SenderName, options.SenderEmail));
            foreach (var email in emailAddresses)
            {
                message.To.Add(MailboxAddress.Parse(email));
            }

            message.Subject = subject;
            message.Body = new TextPart("html")
            {
                Text = body
            };

            await SendEmailAsync(message);
        }

        private async Task SendEmailAsync(MimeMessage message)
        {
            using var client = new SmtpClient
            {
                ServerCertificateValidationCallback = (sender, certificate, chain, errors) => true
            };

            logger.LogInformation($"Smtp client was created");

            await ConfigureSettings(client);

            logger.LogInformation($"Smtp client starts sending...");
            await client.SendAsync(message);
            logger.LogInformation($"Smtp client finished sending");

            await client.DisconnectAsync(true);
            logger.LogInformation($"Smtp client was successfully disconnected");
        }

        private async Task ConfigureSettings(IMailService client)
        {
            var secureSocketOption = options.SecureSocketOption switch
            {
                0 => SecureSocketOptions.None,
                1 => SecureSocketOptions.Auto,
                2 => SecureSocketOptions.SslOnConnect,
                3 => SecureSocketOptions.StartTls,
                4 => SecureSocketOptions.StartTlsWhenAvailable,
                _ => SecureSocketOptions.None
            };

            logger.LogInformation($"Smtp client secure socket option is {secureSocketOption}");


            await client.ConnectAsync(options.Server, options.Port, secureSocketOption);
            logger.LogInformation($"Smtp client connection established");

            client.AuthenticationMechanisms.Remove("XOAUTH2");
            if (options.NeedAuth)
            {
                await client.AuthenticateAsync(options.Username, options.Password);
                logger.LogInformation($"Smtp client authenticated");
            }
        }
    }
}