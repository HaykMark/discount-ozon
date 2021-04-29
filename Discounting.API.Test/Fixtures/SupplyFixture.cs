using System;
using System.Collections.Generic;
using System.Linq;
using Discounting.API.Common.ViewModels;
using Discounting.Entities;
using Discounting.Seeding.Constants;
using Discounting.Tests.Fixtures.Common;
using Discounting.Tests.Infrastructure;

namespace Discounting.Tests.Fixtures
{
    public class SupplyFixture : BaseFixture
    {
        public SupplyFixture(AppState appState) : base(appState)
        {
        }

        public List<SupplyDTO> GetBuyerWithSellerPayload()
        {
            return new List<SupplyDTO>
            {
                new SupplyDTO
                {
                    Type = SupplyType.Torg12,
                    Number = "test_torg-1",
                    Date = DateTime.UtcNow.Date,
                    ContractNumber = "test",
                    ContractDate = DateTime.UtcNow,
                    Amount = 123.11M,
                    DelayEndDate = DateTime.UtcNow.AddDays(100),
                    SellerTin = TestConstants.TestSellerTin,
                    BuyerTin = TestConstants.TestBuyerTin
                },
                new SupplyDTO
                {
                    Type = SupplyType.Invoice,
                    Number = "test_invoice-1",
                    ContractNumber = "test",
                    ContractDate = DateTime.UtcNow,
                    Date = DateTime.UtcNow,
                    Amount = 123.11M,
                    DelayEndDate = DateTime.UtcNow.AddDays(100),
                    BaseDocumentNumber = "test_torg-1",
                    BaseDocumentDate = DateTime.UtcNow.Date,
                    BaseDocumentType = SupplyType.Torg12,
                    SellerTin = TestConstants.TestSellerTin,
                    BuyerTin = TestConstants.TestBuyerTin
                },
                new SupplyDTO
                {
                    Type = SupplyType.Upd,
                    Number = "test_upd-1",
                    ContractNumber = "test",
                    ContractDate = DateTime.UtcNow,
                    Date = DateTime.UtcNow.Date,
                    Amount = 123.11M,
                    DelayEndDate = DateTime.UtcNow.AddDays(100),
                    SellerTin = TestConstants.TestSellerTin,
                    BuyerTin = TestConstants.TestBuyerTin
                },
                new SupplyDTO
                {
                    Type = SupplyType.Ukd,
                    Number = "test_ukd-1",
                    ContractNumber = "test",
                    ContractDate = DateTime.UtcNow,
                    Date = DateTime.UtcNow,
                    Amount = 123.11M,
                    DelayEndDate = DateTime.UtcNow.AddDays(100),
                    BaseDocumentId = GuidValues.SupplyGuids.Ukd,
                    BaseDocumentNumber = "test_upd-1",
                    BaseDocumentDate = DateTime.UtcNow.Date,
                    BaseDocumentType = SupplyType.Upd,
                    SellerTin = TestConstants.TestSellerTin,
                    BuyerTin = TestConstants.TestBuyerTin
                },
                new SupplyDTO
                {
                    Type = SupplyType.Akt,
                    Number = "test_akt-1",
                    ContractNumber = "test",
                    ContractDate = DateTime.UtcNow,
                    Date = DateTime.UtcNow.Date,
                    Amount = 123.11M,
                    DelayEndDate = DateTime.UtcNow.AddDays(100),
                    SellerTin = TestConstants.TestSellerTin,
                    BuyerTin = TestConstants.TestBuyerTin
                },
                new SupplyDTO
                {
                    Type = SupplyType.Invoice,
                    Number = "test_invoice-2",
                    ContractNumber = "test",
                    ContractDate = DateTime.UtcNow,
                    Date = DateTime.UtcNow,
                    Amount = 123.11M,
                    DelayEndDate = DateTime.UtcNow.AddDays(100),
                    BaseDocumentNumber = "test_akt-1",
                    BaseDocumentDate = DateTime.UtcNow.Date,
                    BaseDocumentType = SupplyType.Akt,
                    SellerTin = TestConstants.TestSellerTin,
                    BuyerTin = TestConstants.TestBuyerTin
                }
            };
        }

        public List<SupplyDTO> GetWrongPayload()
        {
            return new List<SupplyDTO>
            {
                new SupplyDTO
                {
                    Type = SupplyType.Torg12,
                    Number = "torg-1",
                    ContractNumber = "test",
                    ContractDate = DateTime.UtcNow,
                    Date = DateTime.UtcNow.Date,
                    Amount = 123.11M,
                    DelayEndDate = DateTime.UtcNow.AddDays(100),
                    SellerTin = TestConstants.TestSellerTin,
                    BuyerTin = TestConstants.TestBuyerTin
                },
                new SupplyDTO
                {
                    Type = SupplyType.Invoice,
                    Number = "test_invoice-2",
                    ContractNumber = "test",
                    ContractDate = DateTime.UtcNow,
                    Date = DateTime.UtcNow,
                    Amount = 123.11M,
                    DelayEndDate = DateTime.UtcNow.AddDays(100),
                    BaseDocumentNumber = "test_torg-1",
                    BaseDocumentDate = DateTime.UtcNow.Date,
                    BaseDocumentType = SupplyType.Torg12,
                    SellerTin = TestConstants.TestBuyerTin,
                    BuyerTin = TestConstants.TestBuyerTin
                },
                new SupplyDTO
                {
                    Type = SupplyType.Ukd,
                    Number = "test_ukd-2",
                    ContractNumber = "test",
                    ContractDate = DateTime.UtcNow,
                    Date = DateTime.UtcNow,
                    Amount = 123.11M,
                    DelayEndDate = DateTime.UtcNow.AddDays(100),
                    BaseDocumentId = GuidValues.SupplyGuids.Ukd,
                    SellerTin = TestConstants.TestSellerTin,
                    BuyerTin = TestConstants.TestBuyerTin
                },
                new SupplyDTO
                {
                    Type = SupplyType.Akt,
                    Number = "test_akt-1",
                    ContractNumber = "test",
                    ContractDate = DateTime.UtcNow,
                    Date = DateTime.UtcNow.Date,
                    Amount = 0,
                    DelayEndDate = DateTime.UtcNow.AddDays(100),
                    SellerTin = TestConstants.TestSellerTin,
                    BuyerTin = TestConstants.TestBuyerTin
                },
                new SupplyDTO
                {
                    Type = SupplyType.Invoice,
                    Number = "test_invoice-3",
                    ContractNumber = "test",
                    ContractDate = DateTime.UtcNow,
                    Date = DateTime.UtcNow,
                    Amount = 123.11M,
                    DelayEndDate = DateTime.UtcNow.AddDays(100),
                    BaseDocumentNumber = "test_akt-1",
                    BaseDocumentDate = null,
                    BaseDocumentType = SupplyType.Torg12,
                    SellerTin = TestConstants.TestSellerTin,
                    BuyerTin = TestConstants.TestBuyerTin
                },
                new SupplyDTO
                {
                    Type = SupplyType.Invoice,
                    Number = "test_invoice-4",
                    ContractNumber = "test",
                    ContractDate = DateTime.UtcNow,
                    Date = DateTime.UtcNow,
                    Amount = 123.11M,
                    DelayEndDate = DateTime.UtcNow.AddDays(100),
                    BaseDocumentNumber = "test_upd-invoice",
                    BaseDocumentDate = DateTime.UtcNow.Date,
                    BaseDocumentType = SupplyType.Upd,
                    SellerTin = TestConstants.TestSellerTin,
                    BuyerTin = TestConstants.TestBuyerTin
                }
            };
        }

        public List<SupplyDTO> ChildSuppliesPayload()
        {
            return new List<SupplyDTO>
            {
                new SupplyDTO
                {
                    Type = SupplyType.Invoice,
                    Number = "test_invoice-no-main-1",
                    ContractNumber = "test",
                    ContractDate = DateTime.UtcNow,
                    Date = DateTime.UtcNow,
                    Amount = 123.11M,
                    DelayEndDate = DateTime.UtcNow.AddDays(100),
                    BaseDocumentNumber = "test_torg-main-1",
                    BaseDocumentDate = DateTime.UtcNow.Date,
                    BaseDocumentType = SupplyType.Torg12,
                    SellerTin = TestConstants.TestSellerTin,
                    BuyerTin = TestConstants.TestBuyerTin
                },
                new SupplyDTO
                {
                    Type = SupplyType.Invoice,
                    Number = "test_invoice-no-main-2",
                    ContractNumber = "test",
                    ContractDate = DateTime.UtcNow,
                    Date = DateTime.UtcNow,
                    Amount = 123.11M,
                    DelayEndDate = DateTime.UtcNow.AddDays(100),
                    BaseDocumentNumber = "test_akt-main-1",
                    BaseDocumentDate = DateTime.UtcNow.Date,
                    BaseDocumentType = SupplyType.Akt,
                    SellerTin = TestConstants.TestSellerTin,
                    BuyerTin = TestConstants.TestBuyerTin
                },
                new SupplyDTO
                {
                    Type = SupplyType.Ukd,
                    Number = "test_ukd-no-main-1",
                    ContractNumber = "test",
                    ContractDate = DateTime.UtcNow,
                    Date = DateTime.UtcNow,
                    Amount = 123.11M,
                    DelayEndDate = DateTime.UtcNow.AddDays(100),
                    BaseDocumentNumber = "test_upd-main-1",
                    BaseDocumentDate = DateTime.UtcNow.Date,
                    BaseDocumentType = SupplyType.Upd,
                    SellerTin = TestConstants.TestSellerTin,
                    BuyerTin = TestConstants.TestBuyerTin
                }
            };
        }

        public List<SupplyDTO> MainSuppliesPayload()
        {
            return new List<SupplyDTO>
            {
                new SupplyDTO
                {
                    Type = SupplyType.Torg12,
                    Number = "test_torg-main-1",
                    ContractNumber = "test",
                    ContractDate = new DateTime(2020, 10, 16),
                    Date = DateTime.UtcNow,
                    Amount = 123.11M,
                    DelayEndDate =  DateTime.UtcNow.AddDays(20),
                    SellerTin = TestConstants.TestSellerTin,
                    BuyerTin = TestConstants.TestBuyerTin
                },
                new SupplyDTO
                {
                    Type = SupplyType.Akt,
                    Number = "test_akt-main-1",
                    ContractNumber = "test",
                    ContractDate = new DateTime(2020, 10, 16),
                    Date = DateTime.UtcNow,
                    Amount = 123.11M,
                    DelayEndDate = DateTime.UtcNow.AddDays(20),
                    SellerTin = TestConstants.TestSellerTin,
                    BuyerTin = TestConstants.TestBuyerTin
                },
                new SupplyDTO
                {
                    Type = SupplyType.Upd,
                    Number = "test_upd-main-1",
                    ContractNumber = "test",
                    ContractDate = new DateTime(2020, 10, 16),
                    Date = DateTime.UtcNow,
                    Amount = 123.11M,
                    DelayEndDate =  DateTime.UtcNow.AddDays(20),
                    SellerTin = TestConstants.TestSellerTin,
                    BuyerTin = TestConstants.TestBuyerTin
                }
            };
        }

        public bool ValidateResponse(List<SupplyDTO> supplies)
        {
            var mainSupplies = supplies.Where(s => Supply.IsMainType(s.Type));
            var childSupplies = supplies.Where(s => !Supply.IsMainType(s.Type));
            return childSupplies.All(childSupply => childSupply.BaseDocumentDate.HasValue &&
                                                    mainSupplies.Any(s => s.Id == childSupply.BaseDocumentId &&
                                                                          s.Number == childSupply.BaseDocumentNumber &&
                                                                          s.Type == childSupply.BaseDocumentType &&
                                                                          s.Date == childSupply.BaseDocumentDate
                                                                              .Value));
        }
    }
}