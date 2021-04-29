using System;
using Discounting.Entities;
using Discounting.Seeding.Constants;

namespace Discounting.Seeding.Seed
{
    public class SupplySeed : ISeedDataStrategy<Supply>
    {
        public Supply[] GetSeedData()
        {
            return new[]
            {
                new Supply
                {
                    Id = GuidValues.SupplyGuids.Torg12,
                    Type = SupplyType.Torg12,
                    Number = "torg-1",
                    Date = DateTime.UtcNow.Date,
                    ContractNumber = "test-contract",
                    ContractDate = DateTime.UtcNow,
                    Amount = 123.11M,
                    ContractId = GuidValues.ContractGuids.TestBuyer,
                    DelayEndDate = DateTime.UtcNow.AddDays(100),
                    SupplyId = GuidValues.SupplyGuids.TorgSupplyId,
                    Provider = SupplyProvider.Manually,
                    CreationDate = DateTime.UtcNow,
                    Status = SupplyStatus.InProcess,
                    CreatorId = GuidValues.UserGuids.TestBuyer
                },
                new Supply
                {
                    Id = GuidValues.SupplyGuids.Invoice,
                    Type = SupplyType.Invoice,
                    Number = "invoice-1",
                    Date = DateTime.UtcNow,
                    ContractNumber = "test-contract",
                    ContractDate = DateTime.UtcNow,
                    Amount = 123.11M,
                    ContractId = GuidValues.ContractGuids.TestBuyer,
                    DelayEndDate = DateTime.UtcNow.AddDays(100),
                    SupplyId = GuidValues.SupplyGuids.TorgSupplyId,
                    Provider = SupplyProvider.Manually,
                    CreationDate = DateTime.UtcNow,
                    Status = SupplyStatus.InProcess,
                    BaseDocumentId = GuidValues.SupplyGuids.Torg12,
                    BaseDocumentNumber = "torg-1",
                    BaseDocumentDate = DateTime.UtcNow.Date,
                    BaseDocumentType = SupplyType.Torg12,
                    CreatorId = GuidValues.UserGuids.TestBuyer
                },
                new Supply
                {
                    Id = GuidValues.SupplyGuids.Upd,
                    Type = SupplyType.Upd,
                    Number = "upd-1",
                    Date = DateTime.UtcNow.Date,
                    ContractNumber = "test-contract",
                    ContractDate = DateTime.UtcNow,
                    Amount = 123.11M,
                    ContractId = GuidValues.ContractGuids.TestBuyer,
                    DelayEndDate = DateTime.UtcNow.AddDays(100),
                    SupplyId = GuidValues.SupplyGuids.UpdSupplyId,
                    Provider = SupplyProvider.Manually,
                    CreationDate = DateTime.UtcNow,
                    Status = SupplyStatus.InProcess,
                    CreatorId = GuidValues.UserGuids.TestBuyer
                },
                new Supply
                {
                    Id = GuidValues.SupplyGuids.Ukd,
                    Type = SupplyType.Ukd,
                    Number = "ukd-1",
                    Date = DateTime.UtcNow,
                    ContractNumber = "test-contract",
                    ContractDate = DateTime.UtcNow,
                    Amount = 123.11M,
                    ContractId = GuidValues.ContractGuids.TestBuyer,
                    DelayEndDate = DateTime.UtcNow.AddDays(100),
                    SupplyId = GuidValues.SupplyGuids.UpdSupplyId,
                    Provider = SupplyProvider.Manually,
                    CreationDate = DateTime.UtcNow,
                    Status = SupplyStatus.InProcess,
                    BaseDocumentId = GuidValues.SupplyGuids.Ukd,
                    BaseDocumentNumber = "upd-1",
                    BaseDocumentDate = DateTime.UtcNow.Date,
                    BaseDocumentType = SupplyType.Upd,
                    CreatorId = GuidValues.UserGuids.TestBuyer
                },
                new Supply
                {
                    Id = GuidValues.SupplyGuids.Akt,
                    Type = SupplyType.Akt,
                    Number = "akt-1",
                    Date = DateTime.UtcNow.Date,
                    ContractNumber = "test-contract",
                    ContractDate = DateTime.UtcNow,
                    Amount = 123.11M,
                    ContractId = GuidValues.ContractGuids.TestBuyer,
                    DelayEndDate = DateTime.UtcNow.AddDays(100),
                    SupplyId = GuidValues.SupplyGuids.AktSupplyId,
                    Provider = SupplyProvider.Manually,
                    CreationDate = DateTime.UtcNow,
                    Status = SupplyStatus.InProcess,
                    CreatorId = GuidValues.UserGuids.TestBuyer
                }
            };
        }
    }
}