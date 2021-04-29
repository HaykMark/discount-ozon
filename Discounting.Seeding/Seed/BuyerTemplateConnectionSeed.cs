using System;
using Discounting.Entities.Templates;
using Discounting.Seeding.Constants;

namespace Discounting.Seeding.Seed
{
    public class BuyerTemplateConnectionSeed : ISeedDataStrategy<BuyerTemplateConnection>
    {
        public BuyerTemplateConnection[] GetSeedData()
        {
            return new[]
            {
                new BuyerTemplateConnection
                {
                    Id = GuidValues.BuyerTemplateConnectionGuids.TestBuyerRegistry,
                    BuyerId = GuidValues.CompanyGuids.TestBuyer,
                    BankId = GuidValues.CompanyGuids.BankUserOne,
                    TemplateId = GuidValues.TemplateGuids.Registry
                },
                new BuyerTemplateConnection
                {
                    Id = GuidValues.BuyerTemplateConnectionGuids.TestBuyerVerification,
                    BuyerId = GuidValues.CompanyGuids.TestBuyer,
                    BankId = GuidValues.CompanyGuids.BankUserOne,
                    TemplateId = GuidValues.TemplateGuids.Verification
                }
            };
        }
    }
}