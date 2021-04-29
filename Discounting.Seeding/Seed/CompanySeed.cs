using System;
using Discounting.Entities;
using Discounting.Entities.CompanyAggregates;
using Discounting.Seeding.Constants;

namespace Discounting.Seeding.Seed
{
    public class CompanySeed : ISeedDataStrategy<Company>
    {
        public Company[] GetSeedData()
        {
            return new[]
            {
                new Company
                {
                    Id = GuidValues.CompanyGuids.Admin,
                    CompanyType = CompanyType.SellerBuyer,
                    ShortName = "Finance trade for admin",
                    FullName = "LLP Finance trade for admin",
                    TIN = "1234567890",
                    RegisteringAuthorityName = "admin authority name",
                    RegistrationStatePlace = "admin registration state place",
                    StateRegistrationDate = DateTime.UtcNow.AddYears(-1),
                    StateStatisticsCode = "admin state code",
                    IncorporationForm = "admin incorporation form",
                    PaidUpAuthorizedCapitalInformation = "admin Paid Up Authorized Capital Information",
                    PSRN = "1234567891234",
                    IsActive = true
                },
                new Company
                {
                    Id = GuidValues.CompanyGuids.TestSeller,
                    CompanyType = CompanyType.SellerBuyer,
                    ShortName = "Finance trade for seller",
                    FullName = "LLP Finance trade for seller",
                    TIN = "0000000000",
                    RegisteringAuthorityName = "seller authority name",
                    RegistrationStatePlace = "seller registration state place",
                    StateRegistrationDate = DateTime.UtcNow.AddYears(-1),
                    StateStatisticsCode = "seller state code",
                    PaidUpAuthorizedCapitalInformation = "seller Paid Up Authorized Capital Information",
                    PSRN = "1234567891111",
                    IncorporationForm = "seller incorporation form",
                    IsActive = true
                },
                new Company
                {
                    Id = GuidValues.CompanyGuids.TestBuyer,
                    CompanyType = CompanyType.SellerBuyer,
                    ShortName = "Finance trade for buyer",
                    FullName = "LLP Finance trade for buyer",
                    TIN = "1111111111",
                    RegisteringAuthorityName = "buyer authority name",
                    RegistrationStatePlace = "buyer registration state place",
                    StateRegistrationDate = DateTime.UtcNow.AddYears(-1),
                    StateStatisticsCode = "buyer state code",
                    PaidUpAuthorizedCapitalInformation = "buyer Paid Up Authorized Capital Information",
                    PSRN = "1234567890000",
                    IncorporationForm = "buyer incorporation form",
                    IsActive = true
                },
                new Company
                {
                    Id = GuidValues.CompanyGuids.TestSimpleUser,
                    CompanyType = CompanyType.SellerBuyer,
                    ShortName = "Finance trade for client",
                    FullName = "LLP Finance trade for client",
                    TIN = "2222222222",
                    RegisteringAuthorityName = "simple user authority name",
                    RegistrationStatePlace = "simple user registration state place",
                    StateRegistrationDate = DateTime.UtcNow.AddYears(-1),
                    StateStatisticsCode = "simple user state code",
                    PaidUpAuthorizedCapitalInformation = "simple user Paid Up Authorized Capital Information",
                    PSRN = "1234567890001",
                    IncorporationForm = "simple user incorporation form",
                    IsActive = true
                },
                new Company
                {
                    Id = GuidValues.CompanyGuids.BankUserOne,
                    CompanyType = CompanyType.Bank,
                    ShortName = "Finance trade for Bank",
                    FullName = "LLP Finance trade for Bank",
                    TIN = "3333333333",
                    RegisteringAuthorityName = "bank user authority name",
                    RegistrationStatePlace = "bank user registration state place",
                    StateRegistrationDate = DateTime.UtcNow.AddYears(-1),
                    StateStatisticsCode = "bank user state code",
                    PaidUpAuthorizedCapitalInformation = "bank Paid Up Authorized Capital Information",
                    IncorporationForm = "bank incorporation form",
                    PSRN = "1234567893333",
                    IsActive = true
                },
                new Company
                {
                    Id = GuidValues.CompanyGuids.BankUserSecond,
                    CompanyType = CompanyType.Bank,
                    ShortName = "Finance trade for Bank2",
                    FullName = "LLP Finance trade for Bank2",
                    TIN = "3333333334",
                    RegisteringAuthorityName = "bank2 user authority name",
                    RegistrationStatePlace = "bank2 user registration state place",
                    StateRegistrationDate = DateTime.UtcNow.AddYears(-1),
                    StateStatisticsCode = "bank2 user state code",
                    PaidUpAuthorizedCapitalInformation = "bank2 Paid Up Authorized Capital Information",
                    IncorporationForm = "bank 2 incorporation form",
                    PSRN = "1234567893334",
                    IsActive = true
                }
            };
        }
    }
}