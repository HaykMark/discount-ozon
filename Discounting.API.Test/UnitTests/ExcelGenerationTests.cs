using System;
using System.Collections.Generic;
using System.IO;
using Discounting.Data.Context;
using Discounting.Entities;
using Discounting.Entities.CompanyAggregates;
using Discounting.Entities.TariffDiscounting;
using Discounting.Entities.Templates;
using Discounting.Logics;
using Discounting.Logics.Excel;
using Discounting.Seeding.Constants;
using Moq;
using Xunit;

namespace Discounting.Tests.UnitTests
{
    public class ExcelGenerationTests
    {
        private static Registry CreateTestRegistryData(
            FinanceType financeType = FinanceType.SupplyVerification)
        {
            return new Registry
            {
                Number = 1,
                Date = DateTime.Now,
                Amount = 1234.12M,
                BankId = Guid.NewGuid(),
                Bank = new Company
                {
                    TIN = "bank tin",
                    FullName = "bank full name",
                    ShortName = "bank short name",
                    KPP = "Bank kpp",
                    OwnerFullName = "bank owner full name",
                    OwnerPosition = "bank owner position"
                },
                Contract = new Contract
                {
                    IsFactoring = financeType == FinanceType.SupplyVerification,
                    IsDynamicDiscounting = financeType == FinanceType.DynamicDiscounting,
                    Seller = new Company
                    {
                        TIN = "seller tin",
                        FullName = "Seller full name",
                        ShortName = "Seller short name",
                        KPP = "Seller kpp",
                        OwnerFullName = "Seller owner full name",
                        OwnerPosition = "Seller owner position"
                    },
                    Buyer = new Company
                    {
                        TIN = "Buyer tin",
                        FullName = "Buyer full name",
                        ShortName = "Buyer short name",
                        KPP = "Buyer kpp",
                        OwnerFullName = "Buyer owner full name",
                        OwnerPosition = "Buyer owner position"
                    }
                },
                Discount = new Discount
                {
                    Rate = 10,
                    DiscountedAmount = 500,
                    DiscountingSource = DiscountingSource.Personal,
                    AmountToPay = 700,
                    PlannedPaymentDate = DateTime.UtcNow.AddDays(5)
                },
                Supplies = new List<Supply>
                {
                    new Supply
                    {
                        Number = "1",
                        Date = DateTime.UtcNow,
                        Amount = 12.34M,
                        ContractNumber = "c1",
                        ContractDate = DateTime.UtcNow,
                        DelayEndDate = DateTime.UtcNow,
                        Type = SupplyType.Torg12,
                        SupplyDiscount = new SupplyDiscount
                        {
                            Rate = 5,
                            DiscountedAmount = 50
                        }
                    },
                    new Supply
                    {
                        Number = "2",
                        Date = DateTime.UtcNow,
                        Amount = 12.34M,
                        ContractNumber = "c2",
                        DelayEndDate = DateTime.UtcNow,
                        ContractDate = DateTime.UtcNow,
                        Type = SupplyType.Upd,
                        SupplyDiscount = new SupplyDiscount
                        {
                            Rate = 10,
                            DiscountedAmount = 100
                        }
                    },
                    new Supply
                    {
                        Number = "3",
                        Date = DateTime.UtcNow,
                        Amount = 12.34M,
                        ContractNumber = "c3",
                        DelayEndDate = DateTime.UtcNow,
                        ContractDate = DateTime.UtcNow,
                        Type = SupplyType.Akt,
                        SupplyDiscount = new SupplyDiscount
                        {
                            Rate = 15,
                            DiscountedAmount = 200
                        }
                    }
                }
            };
        }

        [Fact]
        public void Generate_Discount_Excel_Generated()
        {
            //Arrange
            var dir = Path.Combine(
                new DirectoryInfo(Directory.GetCurrentDirectory()).Parent.Parent.Parent.FullName,
                "TestFiles");
            var registry = CreateTestRegistryData(FinanceType.DynamicDiscounting);
            var fileName = registry.GetFileName(TemplateType.Discount);
            Assert.False(string.IsNullOrEmpty(fileName));
            var pathToSave = $@"E:\Soft\DiscountNew\Discounting\Discounting.API.Test\{fileName}";
            var registryReport = ExcelFactory.CreateExcelReport(registry, ExcelReportType.Registry);
            var unitOfWork = new Mock<IUnitOfWork>();
            var uploadPathProviderService = new Mock<IUploadPathProviderService>();
            var excelService = new ExcelDocumentGeneratorService(unitOfWork.Object, uploadPathProviderService.Object);
            excelService.CreateExcelViaTemplate(registryReport,
                $@"E:\Soft\DiscountNew\Discounting\Discounting.API\Uploads\DiscountTemplates\{GuidValues.TemplateGuids.Discount}.xlsx",
                pathToSave);
            Assert.True(File.Exists(pathToSave));
            File.Delete(pathToSave);
        }

        [Fact]
        public void Generate_Registry_Excel_Generated()
        {
            //Arrange
            var dir = Path.Combine(
                new DirectoryInfo(Directory.GetCurrentDirectory()).Parent.Parent.Parent.FullName,
                "TestFiles");
            var registry = CreateTestRegistryData();
            var fileName = registry.GetFileName(TemplateType.Registry);
            Assert.False(string.IsNullOrEmpty(fileName));
            var pathToSave = $@"E:\Soft\DiscountNew\Discounting\Discounting.API.Test\{fileName}";
            var registryReport = ExcelFactory.CreateExcelReport(registry, ExcelReportType.Registry);
            var unitOfWork = new Mock<IUnitOfWork>();
            var uploadPathProviderService = new Mock<IUploadPathProviderService>();
            var excelService = new ExcelDocumentGeneratorService(unitOfWork.Object, uploadPathProviderService.Object);
            excelService.CreateExcelViaTemplate(registryReport,
                $@"E:\Soft\DiscountNew\Discounting\Discounting.API\Uploads\RegistryTemplates\{GuidValues.TemplateGuids.Registry}.xlsx",
                pathToSave);
            Assert.True(File.Exists(pathToSave));
            File.Delete(pathToSave);
        }

        [Fact]
        public void Generate_Verification_Excel_Generated()
        {
            //Arrange
            var dir = Path.Combine(
                new DirectoryInfo(Directory.GetCurrentDirectory()).Parent.Parent.Parent.FullName,
                "TestFiles");
            var registry = CreateTestRegistryData();
            var fileName = registry.GetFileName(TemplateType.Verification);
            Assert.False(string.IsNullOrEmpty(fileName));
            var pathToSave = $@"E:\Soft\DiscountNew\Discounting\Discounting.API.Test\{fileName}";
            var registryReport = ExcelFactory.CreateExcelReport(registry, ExcelReportType.Registry);
            var unitOfWork = new Mock<IUnitOfWork>();
            var uploadPathProviderService = new Mock<IUploadPathProviderService>();
            var excelService = new ExcelDocumentGeneratorService(unitOfWork.Object, uploadPathProviderService.Object);
            excelService.CreateExcelViaTemplate(registryReport,
                $@"E:\Soft\DiscountNew\Discounting\Discounting.API\Uploads\VerificationTemplates\{GuidValues.TemplateGuids.Verification}.xlsx",
                pathToSave);
            Assert.True(File.Exists(pathToSave));
            File.Delete(pathToSave);
        }
    }
}