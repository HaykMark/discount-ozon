using System;
using Discounting.Entities.Account;
using Discounting.Seeding.Constants;

namespace Discounting.Seeding.Seed
{
    public class UserSeed : ISeedDataStrategy<User>
    {
        public User[] GetSeedData()
        {
            return new[]
            {
                new User
                {
                    Id = GuidValues.UserGuids.Admin,
                    Email = "monitoringetp@yandex.ru",
                    DisplayName = "Admin",
                    FirstName = "Admin",
                    Surname = "Adminyan",
                    SecondName = "Amini",
                    IsActive = true,
                    LastLoggedIn = null,
                    CanSign = true,
                    Salt =
                        @"ZVmu8znaXa6hOfCZn3+nDFgQzxN/OXV5d4FK0rBXsmDbeDEMJ6Fv2P1WITOIvVnvpcntARyih8D/zS7cv8dsRoTEmcKXNx/m40Du1tqQMMq0GUbKuUXviDEzG2EbdIECoVs7GESLPSid5gpmtRI4+Hjjz26q7L053BUcMoMmuA0PAzhlyL8G9VQZwMWzh3YcMX0SsmzrF8QV+eliCpP6hsemOTrPBn/PWdHLIl0w5BsuahaDW1VwevBMzUf2P+g0DoILwfCLlOzej6nfrBHZaZitji+cW16pVquajvA/JIerFq2Lg7Mv7k4iHuQauW4HL9M7eagUvw7iHoO4speBhA==",
                    Password = @"ObNTHy7nb0RaBbpHBllnifDoUtkfn6HZssNNPBiTJegVU116dk5PFfR51jDClJ63ZV0swA/x91iaHybsKz5pNw==",
                    SerialNumber = @"ef786f7326864b60a4c5f991b6ca7487",
                    IsAdmin = true,
                    IsSuperAdmin = true,
                    CreationDate = DateTime.UtcNow,
                    IsConfirmedByAdmin = true,
                    IsEmailConfirmed = true,
                    PasswordRetryLimit = 10,
                    CompanyId = GuidValues.CompanyGuids.Admin
                },
                
                new User
                {
                    Id = GuidValues.UserGuids.TestSeller,
                    Email = "e.postavshik@yandex.ru",
                    DisplayName = "Seller",
                    FirstName = "Seller",
                    Surname = "Selleryan",
                    SecondName = "Selleri",
                    IsActive = true,
                    LastLoggedIn = null,
                    CanSign = true,
                    Salt =
                        @"ZVmu8znaXa6hOfCZn3+nDFgQzxN/OXV5d4FK0rBXsmDbeDEMJ6Fv2P1WITOIvVnvpcntARyih8D/zS7cv8dsRoTEmcKXNx/m40Du1tqQMMq0GUbKuUXviDEzG2EbdIECoVs7GESLPSid5gpmtRI4+Hjjz26q7L053BUcMoMmuA0PAzhlyL8G9VQZwMWzh3YcMX0SsmzrF8QV+eliCpP6hsemOTrPBn/PWdHLIl0w5BsuahaDW1VwevBMzUf2P+g0DoILwfCLlOzej6nfrBHZaZitji+cW16pVquajvA/JIerFq2Lg7Mv7k4iHuQauW4HL9M7eagUvw7iHoO4speBhA==",
                    Password = @"ObNTHy7nb0RaBbpHBllnifDoUtkfn6HZssNNPBiTJegVU116dk5PFfR51jDClJ63ZV0swA/x91iaHybsKz5pNw==",
                    SerialNumber = @"ef786f7326864b60a4c5f991b6ca7487",
                    IsAdmin = true,
                    IsSuperAdmin = false,
                    IsTestUser = true,
                    CreationDate = DateTime.UtcNow,
                    IsConfirmedByAdmin = true,
                    IsEmailConfirmed = true,
                    PasswordRetryLimit = 10,
                    CompanyId = GuidValues.CompanyGuids.TestSeller
                },
                new User
                {
                    Id = GuidValues.UserGuids.TestBuyer,
                    Email = "zakazchik.etp@yandex.ru",
                    DisplayName = "Buyer",
                    FirstName = "Buyer",
                    Surname = "Buyeryan",
                    SecondName = "Buyeri",
                    IsActive = true,
                    LastLoggedIn = null,
                    CanSign = true,
                    Salt =
                        @"ZVmu8znaXa6hOfCZn3+nDFgQzxN/OXV5d4FK0rBXsmDbeDEMJ6Fv2P1WITOIvVnvpcntARyih8D/zS7cv8dsRoTEmcKXNx/m40Du1tqQMMq0GUbKuUXviDEzG2EbdIECoVs7GESLPSid5gpmtRI4+Hjjz26q7L053BUcMoMmuA0PAzhlyL8G9VQZwMWzh3YcMX0SsmzrF8QV+eliCpP6hsemOTrPBn/PWdHLIl0w5BsuahaDW1VwevBMzUf2P+g0DoILwfCLlOzej6nfrBHZaZitji+cW16pVquajvA/JIerFq2Lg7Mv7k4iHuQauW4HL9M7eagUvw7iHoO4speBhA==",
                    Password = @"ObNTHy7nb0RaBbpHBllnifDoUtkfn6HZssNNPBiTJegVU116dk5PFfR51jDClJ63ZV0swA/x91iaHybsKz5pNw==",
                    SerialNumber = @"ef786f7326864b60a4c5f991b6ca7487",
                    IsAdmin = true,
                    IsSuperAdmin = false,
                    IsTestUser = true,
                    CreationDate = DateTime.UtcNow,
                    IsConfirmedByAdmin = true,
                    IsEmailConfirmed = true,
                    PasswordRetryLimit = 10,
                    CompanyId = GuidValues.CompanyGuids.TestBuyer
                },
                new User
                {
                    Id = GuidValues.UserGuids.TestSimpleUser,
                    Email = "client@discounting.ru",
                    DisplayName = "Simple User",
                    FirstName = "Client",
                    Surname = "Clientyan",
                    SecondName = "Clienti",
                    IsActive = true,
                    LastLoggedIn = null,
                    CanSign = true,
                    Salt =
                        @"ZVmu8znaXa6hOfCZn3+nDFgQzxN/OXV5d4FK0rBXsmDbeDEMJ6Fv2P1WITOIvVnvpcntARyih8D/zS7cv8dsRoTEmcKXNx/m40Du1tqQMMq0GUbKuUXviDEzG2EbdIECoVs7GESLPSid5gpmtRI4+Hjjz26q7L053BUcMoMmuA0PAzhlyL8G9VQZwMWzh3YcMX0SsmzrF8QV+eliCpP6hsemOTrPBn/PWdHLIl0w5BsuahaDW1VwevBMzUf2P+g0DoILwfCLlOzej6nfrBHZaZitji+cW16pVquajvA/JIerFq2Lg7Mv7k4iHuQauW4HL9M7eagUvw7iHoO4speBhA==",
                    Password = @"ObNTHy7nb0RaBbpHBllnifDoUtkfn6HZssNNPBiTJegVU116dk5PFfR51jDClJ63ZV0swA/x91iaHybsKz5pNw==",
                    SerialNumber = @"ef786f7326864b60a4c5f991b6ca7487",
                    IsAdmin = true,
                    IsSuperAdmin = false,
                    IsTestUser = true,
                    CreationDate = DateTime.UtcNow,
                    IsConfirmedByAdmin = true,
                    IsEmailConfirmed = true,
                    PasswordRetryLimit = 10,
                    CompanyId = GuidValues.CompanyGuids.TestSimpleUser
                },
                new User
                {
                    Id = GuidValues.UserGuids.BankUserOne,
                    Email = "b.etp@yandex.ru",
                    DisplayName = "Bank User",
                    FirstName = "Bank",
                    Surname = "Bank",
                    SecondName = "Bank",
                    IsActive = true,
                    LastLoggedIn = null,
                    CanSign = true,
                    Salt =
                        @"ZVmu8znaXa6hOfCZn3+nDFgQzxN/OXV5d4FK0rBXsmDbeDEMJ6Fv2P1WITOIvVnvpcntARyih8D/zS7cv8dsRoTEmcKXNx/m40Du1tqQMMq0GUbKuUXviDEzG2EbdIECoVs7GESLPSid5gpmtRI4+Hjjz26q7L053BUcMoMmuA0PAzhlyL8G9VQZwMWzh3YcMX0SsmzrF8QV+eliCpP6hsemOTrPBn/PWdHLIl0w5BsuahaDW1VwevBMzUf2P+g0DoILwfCLlOzej6nfrBHZaZitji+cW16pVquajvA/JIerFq2Lg7Mv7k4iHuQauW4HL9M7eagUvw7iHoO4speBhA==",
                    Password = @"ObNTHy7nb0RaBbpHBllnifDoUtkfn6HZssNNPBiTJegVU116dk5PFfR51jDClJ63ZV0swA/x91iaHybsKz5pNw==",
                    SerialNumber = @"ef786f7326864b60a4c5f991b6ca7487",
                    IsAdmin = true,
                    IsSuperAdmin = false,
                    IsTestUser = true,
                    CreationDate = DateTime.UtcNow,
                    IsConfirmedByAdmin = true,
                    IsEmailConfirmed = true,
                    PasswordRetryLimit = 10,
                    CompanyId = GuidValues.CompanyGuids.BankUserOne
                },
                new User
                {
                    Id = GuidValues.UserGuids.BankUserSecond,
                    Email = "bank2@discounting.ru",
                    DisplayName = "Bank User2",
                    FirstName = "Bank2",
                    Surname = "Bank2",
                    SecondName = "Bank2",
                    IsActive = true,
                    LastLoggedIn = null,
                    CanSign = true,
                    Salt =
                        @"ZVmu8znaXa6hOfCZn3+nDFgQzxN/OXV5d4FK0rBXsmDbeDEMJ6Fv2P1WITOIvVnvpcntARyih8D/zS7cv8dsRoTEmcKXNx/m40Du1tqQMMq0GUbKuUXviDEzG2EbdIECoVs7GESLPSid5gpmtRI4+Hjjz26q7L053BUcMoMmuA0PAzhlyL8G9VQZwMWzh3YcMX0SsmzrF8QV+eliCpP6hsemOTrPBn/PWdHLIl0w5BsuahaDW1VwevBMzUf2P+g0DoILwfCLlOzej6nfrBHZaZitji+cW16pVquajvA/JIerFq2Lg7Mv7k4iHuQauW4HL9M7eagUvw7iHoO4speBhA==",
                    Password = @"ObNTHy7nb0RaBbpHBllnifDoUtkfn6HZssNNPBiTJegVU116dk5PFfR51jDClJ63ZV0swA/x91iaHybsKz5pNw==",
                    SerialNumber = @"ef786f7326864b60a4c5f991b6ca7487",
                    IsAdmin = true,
                    IsSuperAdmin = false,
                    IsTestUser = true,
                    CreationDate = DateTime.UtcNow,
                    IsConfirmedByAdmin = true,
                    IsEmailConfirmed = true,
                    PasswordRetryLimit = 10,
                    CompanyId = GuidValues.CompanyGuids.BankUserSecond
                }
            };
        }
    }
}