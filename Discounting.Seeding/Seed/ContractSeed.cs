using System;
using Discounting.Entities;
using Discounting.Seeding.Constants;

namespace Discounting.Seeding.Seed
{
    public class ContractSeed : ISeedDataStrategy<Contract>
    {
        public Contract[] GetSeedData()
        {
            return new[]
            {
                new Contract
                {
                    Id = GuidValues.ContractGuids.TestSeller,
                    SellerId = GuidValues.CompanyGuids.TestBuyer,
                    BuyerId = GuidValues.CompanyGuids.TestSeller,
                    IsFactoring = true,
                    Status = ContractStatus.Active,
                    Provider = ContractProvider.Manually,
                    CreationDate = DateTime.UtcNow
                    
                },
                new Contract
                {
                    Id = GuidValues.ContractGuids.TestBuyer,
                    SellerId = GuidValues.CompanyGuids.TestSeller,
                    BuyerId = GuidValues.CompanyGuids.TestBuyer,
                    IsFactoring = true,
                    Status = ContractStatus.Active,
                    Provider = ContractProvider.Manually,
                    CreationDate = DateTime.UtcNow
                },
                new Contract
                {
                    Id = GuidValues.ContractGuids.TestSimpleUser,
                    SellerId = GuidValues.CompanyGuids.TestSeller,
                    BuyerId = GuidValues.CompanyGuids.TestSimpleUser,
                    IsFactoring = true,
                    Status = ContractStatus.Active,
                    Provider = ContractProvider.Automatically,
                    CreationDate = DateTime.UtcNow
                }
            };
        }
    }
}