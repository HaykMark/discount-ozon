using System;
using Discounting.Entities.CompanyAggregates;
using Discounting.Seeding.Constants;

namespace Discounting.Seeding.Seed
{
    public class CompanySettingsSeed : ISeedDataStrategy<CompanySettings>
    {
        public CompanySettings[] GetSeedData()
        {
            return new[]
            {
                new CompanySettings
                {
                    Id = GuidValues.CompanySettingsGuids.Admin,
                    CompanyId = GuidValues.CompanyGuids.Admin,
                    UserId = GuidValues.UserGuids.Admin,
                    CreationDate = DateTime.UtcNow,
                    IsAuction = true,
                    IsSendAutomatically = true,
                    ForbidSellerEditTariff = true
                },
                new CompanySettings
                {
                    Id = GuidValues.CompanySettingsGuids.TestSeller,
                    CompanyId = GuidValues.CompanyGuids.TestSeller,
                    UserId = GuidValues.UserGuids.TestSeller,
                    CreationDate = DateTime.UtcNow,
                },
                new CompanySettings
                {
                    Id = GuidValues.CompanySettingsGuids.TestBuyer,
                    CompanyId = GuidValues.CompanyGuids.TestBuyer,
                    UserId = GuidValues.UserGuids.TestBuyer,
                    CreationDate = DateTime.UtcNow,
                    IsAuction = true,
                    IsSendAutomatically = true,
                    ForbidSellerEditTariff = true
                }
            };
        }
    }
}