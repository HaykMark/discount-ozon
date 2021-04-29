using System;
using Discounting.Entities;
using Discounting.Entities.Templates;
using Discounting.Seeding.Constants;

namespace Discounting.Seeding.Seed
{
    public class TemplateSeed : ISeedDataStrategy<Template>
    {
        public Template[] GetSeedData() =>
            new[]
            {
                new Template
                {
                    Id = GuidValues.TemplateGuids.Registry,
                    CompanyId = GuidValues.CompanyGuids.BankUserSecond,
                    Name = "Ф-Реестр.xlsx",
                    Type = TemplateType.Registry,
                    ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    Size = 10672,
                    CreatedDate = DateTime.UtcNow
                },
                new Template
                {
                    Id = GuidValues.TemplateGuids.Verification,
                    CompanyId = GuidValues.CompanyGuids.BankUserSecond,
                    Name = "Ф-Уведомление.xlsx",
                    Type = TemplateType.Verification,
                    ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    Size = 10672,
                    CreatedDate = DateTime.UtcNow
                },
                new Template
                {
                    Id = GuidValues.TemplateGuids.Discount,
                    CompanyId = GuidValues.CompanyGuids.TestBuyer,
                    Name = "Д-Реестр",
                    Type = TemplateType.Discount,
                    ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    Size = 10672,
                    CreatedDate = DateTime.UtcNow
                },
                new Template
                {
                    Id = GuidValues.TemplateGuids.ProfileRegulationSellerBuyer,
                    CompanyId = GuidValues.CompanyGuids.Admin,
                    Name = "Анкета-ЮЛ.xlsx",
                    Type = TemplateType.ProfileRegulationSellerBuyer,
                    ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    Size = 10672,
                    CreatedDate = DateTime.UtcNow
                },
                new Template
                {
                    Id = GuidValues.TemplateGuids.ProfileRegulationPrivatePerson,
                    CompanyId = GuidValues.CompanyGuids.Admin,
                    Name = "Анкета-ИП.xlsx",
                    Type = TemplateType.ProfileRegulationPrivateCompany,
                    ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    Size = 10672,
                    CreatedDate = DateTime.UtcNow
                },
                new Template
                {
                    Id = GuidValues.TemplateGuids.ProfileRegulationBank,
                    CompanyId = GuidValues.CompanyGuids.Admin,
                    Name = "Банк.xlsx",
                    Type = TemplateType.ProfileRegulationBank,
                    ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    Size = 10672,
                    CreatedDate = DateTime.UtcNow
                },
                new Template
                {
                    Id = GuidValues.TemplateGuids.ProfileRegulationUser,
                    CompanyId = GuidValues.CompanyGuids.Admin,
                    Name = "Анкета.xlsx",
                    Type = TemplateType.ProfileRegulationUser,
                    ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    Size = 10672,
                    CreatedDate = DateTime.UtcNow
                }
                
            };
    }
}