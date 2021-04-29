using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discounting.Common.Types;
using Discounting.Data.Context;
using Discounting.Entities;
using Discounting.Entities.Auditing;
using Discounting.Entities.CompanyAggregates;
using Discounting.Entities.TariffDiscounting;
using Discounting.Entities.Templates;
using Discounting.Extensions;
using DocumentFormat.OpenXml.Office2010.ExcelAc;
using Microsoft.EntityFrameworkCore;

namespace Discounting.Logics
{
    public interface IAuditService
    {
        Task<(Audit[], int)> GetAsync(AuditFilter filter);
        Task CreateAsync(Audit entity);

        Task<string> GetMessageAsync<T>(Guid sourceId)
            where T : class, IEntity<Guid>;
    }

    public class AuditService : IAuditService
    {
        private readonly IUnitOfWork unitOfWork;

        public AuditService(IUnitOfWork unitOfWork)
        {
            this.unitOfWork = unitOfWork;
        }

        public async Task<(Audit[], int)> GetAsync(AuditFilter filter)
        {
            var query = unitOfWork.Set<Audit>()
                .Include(u => u.User)
                .ThenInclude(c => c.Company)
                .Where(a =>
                    (!filter.Id.HasValue || a.Id == filter.Id) &&
                    (!filter.Incident.HasValue || a.Incident == filter.Incident) &&
                    (!filter.UserId.HasValue || a.UserId == filter.UserId) &&
                    (!filter.CompanyId.HasValue || a.User.CompanyId == filter.CompanyId) &&
                    (filter.IncidentResult == IncidentResult.None || a.IncidentResult == filter.IncidentResult) &&
                    (!filter.From.HasValue || a.IncidentDate >= filter.From.Value.ToUniversalTime()) &&
                    (!filter.Until.HasValue || a.IncidentDate <= filter.Until.Value.ToUniversalTime())
                )
                .OrderByDescending(e => e.IncidentDate);
            return (await query
                    .Skip(filter.Offset)
                    .Take(filter.Limit)
                    .ToArrayAsync(),
                await query.CountAsync());
        }

        public async Task CreateAsync(Audit entity)
        {
            await unitOfWork.AddAndSaveAsync(entity);
        }

        public async Task<string> GetMessageAsync<T>(Guid sourceId)
            where T : class, IEntity<Guid>
        {
            var entityType = typeof(T);
            if (entityType == typeof(UnformalizedDocument))
            {
                return await GetUnformalizedDocumentMessageAsync(sourceId);
            }

            if (entityType == typeof(Contract))
            {
                return await GetContractMessageAsync(sourceId);
            }

            if (entityType == typeof(Discount))
            {
                return await GetDiscountMessageAsync(sourceId);
            }

            if (entityType == typeof(Registry))
            {
                return await GetRegistryMessageAsync(sourceId);
            }

            if (entityType == typeof(BuyerTemplateConnection))
            {
                return await GetBuyerTemplateMessageAsync(sourceId);
            }

            if (entityType == typeof(CompanySettings))
            {
                return await GetCompanySettingsMessageAsync(sourceId);
            }

            return "";
        }

        private async Task<string> GetUnformalizedDocumentMessageAsync(Guid sourceId)
        {
            var unformalizedDocument = await unitOfWork.Set<UnformalizedDocument>()
                .Include(s => s.Sender)
                .Include(d => d.Decliner)
                .Include(r => r.Receivers)
                .FirstOrDefaultAsync(d => d.Id == sourceId);
            var receivers = new List<string>();
            foreach (var receiver in unformalizedDocument.Receivers)
            {
                receivers.Add(await unitOfWork.Set<Company>()
                    .Where(c => c.Id == receiver.ReceiverId)
                    .Select(s => s.ShortName)
                    .FirstAsync());
            }

            return $"Отправитель {unformalizedDocument.Sender.ShortName}; " +
                   $"Получатель {string.Join(',', receivers)}; " +
                   $"Тип {unformalizedDocument.Type.ToString()}; " +
                   $"Тема {unformalizedDocument.Topic}; " +
                   $"Сообщение {unformalizedDocument.Message}";
        }

        private async Task<string> GetContractMessageAsync(Guid sourceId)
        {
            var builder = new StringBuilder();
            var contract = await unitOfWork.Set<Contract>()
                .Include(c => c.Seller)
                .Include(c => c.Buyer)
                .FirstOrDefaultAsync(c => c.Id == sourceId);
      
            builder.Append($"Название поставщика {contract.Seller.ShortName}; ");
            builder.Append($"Название покупателя {contract.Buyer.ShortName}; ");
            builder.Append("Схема работы ");

            if (contract.IsFactoring)
            {
                builder.Append("Факторинговое финансирование; ");
            }

            if (contract.IsDynamicDiscounting)
            {
                builder.Append("Динамическое дисконтирование; ");
            }

            if (contract.IsRequiredRegistry)
            {
                builder.Append("Обязательное формирование реестра; ");
            }
            
            if (contract.IsRequiredNotification)
            {
                builder.Append("Обязательное формирование уведомления к реестру; ");
            }
            return builder.ToString();
        }

        private async Task<string> GetBuyerTemplateMessageAsync(Guid sourceId)
        {
            var buyerTemplateConnection = await unitOfWork.Set<BuyerTemplateConnection>()
                .Include(b => b.Buyer)
                .Include(b => b.Bank)
                .FirstOrDefaultAsync(d => d.Id == sourceId);
            return $"Название покупателя; {buyerTemplateConnection.Buyer.ShortName}; " +
                   $"Название банка; {buyerTemplateConnection.Bank.ShortName}";
        }

        private async Task<string> GetCompanySettingsMessageAsync(Guid sourceId)
        {
            var builder = new StringBuilder();
            var companySettings = await unitOfWork.Set<CompanySettings>()
                .Include(b => b.Company)
                .FirstOrDefaultAsync(d => d.Id == sourceId);
            builder.Append($"Название; {companySettings.Company.ShortName}; ");
            builder.Append(companySettings.IsSendAutomatically
                ? "Отправить автоматически"
                : "Не отправлять автоматически");

            builder.Append(companySettings.ForbidSellerEditTariff
                ? "Запретить поставщику редактировать тариф"
                : "Разрешить поставщику редактировать тариф");

            return builder.ToString();
        }

        private async Task<string> GetDiscountMessageAsync(Guid sourceId)
        {
            var discount = await unitOfWork.Set<Discount>()
                .Include(d => d.Registry)
                .ThenInclude(r => r.Contract)
                .ThenInclude(c => c.Seller)
                .Include(d => d.Registry)
                .ThenInclude(r => r.Contract)
                .ThenInclude(c => c.Buyer)
                .Include(d => d.Registry)
                .FirstOrDefaultAsync(d => d.Id == sourceId);
            return $"Поставщик {discount.Registry.Contract.Seller.ShortName}; " +
                   $"Покупатель {discount.Registry.Contract.Buyer.ShortName};" +
                   $"Номер реестра; {discount.Registry.Number}" +
                   $"Дата реестра; {discount.Registry.Date.ToRussianDateFormat()}" +
                   $"Сумма реестра; {discount.Registry.Amount}" +
                   $"Сумма к оплате; {discount.AmountToPay}" +
                   $"Скидка %; {discount.Rate}" +
                   $"Дата оплаты {discount.PlannedPaymentDate.ToRussianDateFormat()}";
        }

        private async Task<string> GetRegistryMessageAsync(Guid sourceId)
        {
            var registry = await unitOfWork.Set<Registry>()
                .Include(r => r.Bank)
                .Include(r => r.Contract)
                .ThenInclude(c => c.Seller)
                .Include(r => r.Contract)
                .ThenInclude(c => c.Buyer)
                .FirstOrDefaultAsync(d => d.Id == sourceId);
            return $"Поставщик {registry.Contract.Seller.ShortName}; " +
                   $"Покупатель {registry.Contract.Buyer.ShortName};" +
                   $"Номер реестра {registry.Number};" +
                   $"Дата реестра {registry.Date.ToRussianDateFormat()};" +
                   $"Сумма реестра {registry.Amount};" +
                   $"Банк {registry.Bank?.ShortName}";
        }
    }
}