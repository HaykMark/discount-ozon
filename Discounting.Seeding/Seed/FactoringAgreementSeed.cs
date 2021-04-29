using System;
using System.Collections.Generic;
using Discounting.Entities.CompanyAggregates;
using Discounting.Seeding.Constants;

namespace Discounting.Seeding.Seed
{
    public class FactoringAgreementSeed : ISeedDataStrategy<FactoringAgreement>
    {
        public FactoringAgreement[] GetSeedData()
        {
            return new[]
            {
                new FactoringAgreement
                {
                    Id = GuidValues.FactoringAgreementGuid.TestBuyerBankOne,
                    CompanyId = GuidValues.CompanyGuids.TestBuyer,
                    BankId = GuidValues.CompanyGuids.BankUserOne,
                    IsActive = true,
                    IsConfirmed = true,
                    FactoringContractNumber = "first-bank-contract-number",
                    FactoringContractDate = DateTime.UtcNow.AddDays(1),
                    CreationDate = DateTime.UtcNow,
                    SupplyFactoringAgreements = new List<SupplyFactoringAgreement>
                    {
                        new SupplyFactoringAgreement
                        {
                            Id = GuidValues.SupplyFactoringAgreementGuid.TestBuyerBankOneSupplyOne,
                            Date = DateTime.UtcNow.AddDays(1),
                            Status = SupplyFactoringAgreementStatus.Active,
                            Number = "first-supply-bank-contract-number-one",
                            FactoringAgreementId = GuidValues.FactoringAgreementGuid.TestBuyerBankOne
                        },
                        new SupplyFactoringAgreement
                        {
                            Id = GuidValues.SupplyFactoringAgreementGuid.TestBuyerBankOneSupplyTwo,
                            Date = DateTime.UtcNow.AddDays(1),
                            Status = SupplyFactoringAgreementStatus.Active,
                            Number = "first-supply-bank-contract-number-two",
                            FactoringAgreementId = GuidValues.FactoringAgreementGuid.TestBuyerBankOne
                        }
                    }
                },
                new FactoringAgreement
                {
                    Id = GuidValues.FactoringAgreementGuid.TestSellerBankTwo,
                    CompanyId = GuidValues.CompanyGuids.TestSeller,
                    BankId = GuidValues.CompanyGuids.BankUserSecond,
                    FactoringContractNumber = "second-bank-contract-number",
                    FactoringContractDate = DateTime.UtcNow.AddDays(1),
                    IsActive = true,
                    IsConfirmed = true,
                    CreationDate = DateTime.UtcNow,
                    SupplyFactoringAgreements = new List<SupplyFactoringAgreement>
                    {
                        new SupplyFactoringAgreement
                        {
                            Id = GuidValues.SupplyFactoringAgreementGuid.TestSellerBankTwoSupplyOne,
                            Date = DateTime.UtcNow.AddDays(1),
                            Status = SupplyFactoringAgreementStatus.Active,
                            Number = "second-supply-bank-contract-number-one",
                            FactoringAgreementId = GuidValues.FactoringAgreementGuid.TestSellerBankTwo
                        },
                        new SupplyFactoringAgreement
                        {
                            Id = GuidValues.SupplyFactoringAgreementGuid.TestSellerBankTwoSupplyTwo,
                            Date = DateTime.UtcNow.AddDays(1),
                            Status = SupplyFactoringAgreementStatus.Active,
                            Number = "second-supply-bank-contract-number-two",
                            FactoringAgreementId = GuidValues.FactoringAgreementGuid.TestSellerBankTwo
                        }
                    }
                }
            };
        }
    }
}