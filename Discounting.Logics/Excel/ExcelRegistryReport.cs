using System;
using System.Collections.Generic;
using System.Linq;
using Discounting.Common.Exceptions;
using Discounting.Common.Validation;
using Discounting.Entities;
using Discounting.Entities.Extensions;
using Discounting.Entities.TariffDiscounting;
using Discounting.Extensions;

namespace Discounting.Logics.Excel
{
    public class ExcelRegistryReport : IExcelReport
    {
        public ExcelRegistryReport(Registry registry)
        {
            ValidateReportFields(registry);
            AssignRegistryValues(registry);
            AssignSupplyValues(registry);
            AssignBankInfoValues(registry);
            AssignCompanyInfoValues(registry);
        }

        private void ValidateReportFields(Registry registry)
        {
            if (registry.Contract is null)
            {
                throw new ValidationException(new RequiredFieldValidationError(nameof(Contract)));
            }

            if (registry.Contract.Seller is null)
            {
                throw new ValidationException(new RequiredFieldValidationError("Seller"));
            }

            if (registry.Contract.Buyer is null)
            {
                throw new ValidationException(new RequiredFieldValidationError("Buyer"));
            }

            if (!registry.Supplies.Any())
            {
                throw new ValidationException(new RequiredFieldValidationError(nameof(Supplies)));
            }

            if (registry.FinanceType == FinanceType.DynamicDiscounting)
            {
                if (registry.Discount is null)
                {
                    throw new ValidationException(new RequiredFieldValidationError(nameof(Discount)));
                }

                if (registry.Supplies.Any(s => Supply.IsMainType(s.Type) && s.SupplyDiscount is null))
                {
                    throw new ValidationException(new RequiredFieldValidationError(nameof(SupplyDiscount)));
                }
            }
        }

        private void AssignRegistryValues(Registry registry)
        {
            Number = registry.Number.ToString();
            Date = registry.Date.ToRussianDateFormat();
            Amount = Convert.ToDouble(registry.Amount);
            AmountNds20 = Amount / 1.2 * 0.2;
            AmountRusNds20 = AmountNds20.ToRussianString();
            AmountRus = registry.Amount.ToRussianString();
            SellerName = registry.Contract.Seller.FullName;
            BuyerName = registry.Contract.Buyer.FullName;
            BankName = registry.BankId.HasValue ? registry.Bank.FullName : string.Empty;
            SellerShortName = registry.Contract.Seller.ShortName;
            BuyerShortName = registry.Contract.Buyer.ShortName;
            BankShortName = registry.BankId.HasValue ? registry.Bank.ShortName : string.Empty;
            SellerTin = registry.Contract.Seller.TIN;
            SellerKpp = registry.Contract.Seller.KPP;
            SellerOwnerFullName = registry.Contract.Seller.OwnerFullName;
            BuyerOwnerFullName = registry.Contract.Buyer.OwnerFullName;
            SellerOwnerPosition = registry.Contract.Seller.OwnerPosition;
            BuyerTin = registry.Contract.Buyer.TIN;
            BuyerKpp = registry.Contract.Buyer.KPP;
            BuyerOwnerPosition = registry.Contract.Buyer.OwnerPosition;
            ContractNumber = registry.Supplies.First().ContractNumber;
            ContractDate = registry.Supplies.First().ContractDate.ToRussianDateFormat();
            //Discount
            if (registry.FinanceType == FinanceType.DynamicDiscounting)
            {
                PlannedPaymentDate = registry.Discount.PlannedPaymentDate.ToRussianDateFormat();
                DiscountedAmount = Convert.ToDouble(registry.Discount.DiscountedAmount);
                DiscountedAmountNds20 = DiscountedAmount / 1.2 * 0.2;
                DiscountedAmountRusNds20 = DiscountedAmountNds20.ToRussianString();
                AmountToPay = Convert.ToDouble(registry.Discount.AmountToPay);
                DiscountedAmountRus = DiscountedAmount.ToRussianString();
                AmountToPayRus = AmountToPay.ToRussianString();
                AmountToPayNds20 = AmountToPay / 1.2 * 0.2;
                AmountToPayRusNds20 = AmountToPayNds20.ToRussianString();
            }
        }

        private void AssignCompanyInfoValues(Registry registry)
        {
            SellerOGRN = registry.Contract.Seller.PSRN;
            BuyerOGRN = registry.Contract.Buyer.PSRN;
            if (registry.Contract.Seller.CompanyContactInfo != null)
            {
                SellerLocationAddress = registry.Contract.Seller.CompanyContactInfo.Address;
                SellerActualLocationAddress = registry.Contract.Seller.CompanyContactInfo.OrganizationAddress;
            }

            if (registry.Contract.Buyer.CompanyContactInfo != null)
            {
                BuyerLocationAddress = registry.Contract.Buyer.CompanyContactInfo.Address;
                BuyerActualLocationAddress = registry.Contract.Buyer.CompanyContactInfo.OrganizationAddress;
            }

            if (registry.Contract.Seller.CompanyOwnerPositionInfo != null)
            {
                SellerOwnerSurname = registry.Contract.Seller.CompanyOwnerPositionInfo.LastName;
                SellerOwnerSecondName = registry.Contract.Seller.CompanyOwnerPositionInfo.SecondName;
                SellerOwnerName = registry.Contract.Seller.CompanyOwnerPositionInfo.FirstName;
            }

            if (registry.Contract.Buyer.CompanyOwnerPositionInfo != null)
            {
                BuyerOwnerSurname = registry.Contract.Buyer.CompanyOwnerPositionInfo.LastName;
                BuyerOwnerSecondName = registry.Contract.Buyer.CompanyOwnerPositionInfo.SecondName;
                BuyerOwnerName = registry.Contract.Buyer.CompanyOwnerPositionInfo.FirstName;
            }


            if (registry.Bank != null)
            {
                FactorOGRN = registry.Bank.PSRN;
                if (registry.Bank.CompanyContactInfo != null)
                {
                    FactorLocationAddress = registry.Bank.CompanyContactInfo.Address;
                    FactorActualLocationAddress = registry.Bank.CompanyContactInfo.OrganizationAddress;
                }

                if (registry.Bank.CompanyOwnerPositionInfo != null)
                {
                    FactorOwnerSurname = registry.Bank.CompanyOwnerPositionInfo.LastName;
                    FactorOwnerName = registry.Bank.CompanyOwnerPositionInfo.FirstName;
                    FactorOwnerSecondName = registry.Bank.CompanyOwnerPositionInfo.SecondName;
                }
            }
        }

        private void AssignBankInfoValues(Registry registry)
        {
            if (registry.Contract.Seller.CompanyBankInfos.Any())
            {
                var sellerBankInfo = registry.Contract.Seller.CompanyBankInfos.Single();
                SellerBankName = sellerBankInfo.Name;
                SellerBankAddress = sellerBankInfo.City;
                SellerBankCheckingAccount = sellerBankInfo.CheckingAccount;
                SellerBankCorrespondentAccount = sellerBankInfo.CorrespondentAccount;
                SellerBankBIK = sellerBankInfo.Bic;
                SellerBankOGRN = sellerBankInfo.OGRN;
            }

            if (registry.Contract.Buyer.CompanyBankInfos.Any())
            {
                var buyerBankInfo = registry.Contract.Buyer.CompanyBankInfos.Single();
                BuyerBankName = buyerBankInfo.Name;
                BuyerBankAddress = buyerBankInfo.City;
                BuyerBankCheckingAccount = buyerBankInfo.CheckingAccount;
                BuyerBankCorrespondentAccount = buyerBankInfo.CorrespondentAccount;
                BuyerBankBIK = buyerBankInfo.Bic;
                BuyerBankOGRN = buyerBankInfo.OGRN;
            }

            if (registry.Bank != null && registry.Bank.CompanyBankInfos.Any())
            {
                var factorBankInfo = registry.Bank.CompanyBankInfos.Single();
                FactorBankName = factorBankInfo.Name;
                FactorBankAddress = factorBankInfo.City;
                FactorBankCheckingAccount = factorBankInfo.CheckingAccount;
                FactorBankCorrespondentAccount = factorBankInfo.CorrespondentAccount;
                FactorBankBIK = factorBankInfo.Bic;
                FactorBankOGRN = factorBankInfo.OGRN;
            }


            if (registry.FinanceType == FinanceType.SupplyVerification && registry.FactoringAgreement != null)
            {
                FactoringContractNumber = registry.FactoringAgreement.FactoringContractNumber;
                FactoringContractDate = registry.FactoringAgreement.FactoringContractDate?.ToRussianDateFormat();
                PaymentBankName = registry.FactoringAgreement.BankName;
                PaymentBankAddress = registry.FactoringAgreement.BankCity;
                PaymentBankCheckingAccount = registry.FactoringAgreement.BankCheckingAccount;
                PaymentBankBIK = registry.FactoringAgreement.BankBic;
                PaymentBankOGRN = registry.FactoringAgreement.BankOGRN;
                PaymentBankCorrespondentAccount = registry.FactoringAgreement.BankCorrespondentAccount;
            }
        }

        private void AssignSupplyValues(Registry registry)
        {
            var correctedDocuments = registry.Supplies.Where(s => s.Type == SupplyType.Ukd).ToList();
            Supplies = registry.Supplies
                .Select((s, index) =>
                {
                    double discountAmount = 0;
                    double discountPercentage = 0;
                    double discountPayment = 0;
                    if (registry.FinanceType == FinanceType.DynamicDiscounting &&
                        Supply.IsMainType(s.Type))
                    {
                        var supplyAmount = s.Amount - correctedDocuments.Where(c => c.BaseDocumentId == s.Id)
                            .Select(c => c.Amount)
                            .DefaultIfEmpty(0)
                            .Sum();
                        discountAmount = Convert.ToDouble(supplyAmount - s.SupplyDiscount.DiscountedAmount);
                        discountPercentage = Convert.ToDouble(s.SupplyDiscount.DiscountedAmount * 100 / supplyAmount);
                        discountPayment = 100 - discountPercentage;
                    }

                    return new ExcelGridReport
                    {
                        Index = ++index,
                        SellerTin = registry.Contract.Seller.TIN,
                        BuyerName = registry.Contract.Buyer.FullName,
                        BuyerTin = registry.Contract.Buyer.TIN,
                        ContractNumber = s.ContractNumber,
                        ContractDate = s.ContractDate.ToRussianDateFormat(),
                        SupplyDelayDate = s.DelayEndDate.ToRussianDateFormat(),
                        SupplyAmount = Convert.ToDouble(s.Amount),
                        SupplyAmountRus = s.Amount.ToRussianString(),
                        SupplyAmountNds = Convert.ToDouble(s.Amount) / 1.2 * 0.2,
                        SupplyAmountRusNds = (Convert.ToDouble(s.Amount) / 1.2 * 0.2).ToRussianString(),
                        SupplyNumber = s.Number,
                        SupplyType = s.Type.ToRussianName(),
                        SupplyDate = s.Date.ToRussianDateFormat(),
                        //Discount
                        DiscountedAmount = discountAmount,
                        DiscountedAmountRus = discountAmount.ToRussianString(),
                        DiscountPercentage = discountPercentage,
                        DiscountPercentageRus = discountPercentage.ToRussianString(),
                        DiscountPayment = discountPayment,
                        DiscountPaymentRus = discountPayment.ToRussianString(),
                        DiscountedAmountNds20 = discountAmount / 1.2 * 0.2,
                        DiscountedAmountRusNds20 = (discountAmount / 1.2 * 0.2).ToRussianString(),
                    };
                }).ToList();
        }

        //Made them public for ClosedXML
        public string Number { get; private set; }
        public string Date { get; private set; }
        public double Amount { get; private set; }
        public double AmountNds20 { get; private set; }
        public string AmountRusNds20 { get; private set; }
        public string AmountRus { get; private set; }
        public string SellerName { get; private set; }
        public string BuyerName { get; private set; }
        public string BankName { get; private set; }
        public string SellerShortName { get; private set; }
        public string BuyerShortName { get; private set; }
        public string BankShortName { get; private set; }
        public string SellerTin { get; private set; }
        public string SellerKpp { get; private set; }
        public string SellerOwnerPosition { get; private set; }
        public string BuyerTin { get; private set; }
        public string BuyerKpp { get; private set; }
        public string BuyerOwnerPosition { get; private set; }
        public string ContractNumber { get; set; }
        public string ContractDate { get; set; }

        //Discounting
        public string PlannedPaymentDate { get; private set; }
        public double AmountToPay { get; private set; }
        public double AmountToPayNds20 { get; private set; }
        public string AmountToPayRusNds20 { get; private set; }
        public string AmountToPayRus { get; private set; }
        public double DiscountedAmount { get; private set; }
        public double DiscountedAmountNds20 { get; private set; }
        public string DiscountedAmountRusNds20 { get; private set; }
        public string DiscountedAmountRus { get; private set; }

        //Bank info
        public string SellerBankName { get; set; }
        public string BuyerBankName { get; set; }
        public string FactorBankName { get; set; }
        public string SellerBankAddress { get; set; }
        public string BuyerBankAddress { get; set; }
        public string FactorBankAddress { get; set; }
        public string SellerBankBIK { get; set; }
        public string BuyerBankBIK { get; set; }
        public string FactorBankBIK { get; set; }
        public string SellerBankOGRN { get; set; }
        public string BuyerBankOGRN { get; set; }
        public string FactorBankOGRN { get; set; }
        public string SellerBankCorrespondentAccount { get; set; }
        public string BuyerBankCorrespondentAccount { get; set; }
        public string FactorBankCorrespondentAccount { get; set; }
        public string SellerBankCheckingAccount { get; set; }
        public string BuyerBankCheckingAccount { get; set; }
        public string FactorBankCheckingAccount { get; set; }
        public string FactoringContractNumber { get; set; }
        public string FactoringContractDate { get; set; }
        public string PaymentBankName { get; set; }
        public string PaymentBankAddress { get; set; }
        public string PaymentBankBIK { get; set; }
        public string PaymentBankOGRN { get; set; }
        public string PaymentBankCorrespondentAccount { get; set; }
        public string PaymentBankCheckingAccount { get; set; }

        //Company info
        public string SellerOwnerFullName { get; private set; }
        public string BuyerOwnerFullName { get; private set; }
        public string FactorOwnerFullName { get; private set; }
        public string SellerOGRN { get; set; }
        public string BuyerOGRN { get; set; }
        public string FactorOGRN { get; set; }
        public string SellerLocationAddress { get; set; }
        public string BuyerLocationAddress { get; set; }
        public string FactorLocationAddress { get; set; }
        public string SellerActualLocationAddress { get; set; }
        public string BuyerActualLocationAddress { get; set; }
        public string FactorActualLocationAddress { get; set; }
        public string SellerOwnerSurname { get; set; }
        public string BuyerOwnerSurname { get; set; }
        public string FactorOwnerSurname { get; set; }
        public string SellerOwnerName { get; private set; }
        public string BuyerOwnerName { get; private set; }
        public string FactorOwnerName { get; set; }
        public string SellerOwnerSecondName { get; set; }
        public string BuyerOwnerSecondName { get; set; }
        public string FactorOwnerSecondName { get; set; }


        public List<ExcelGridReport> Supplies { get; private set; }
    }

    public class ExcelGridReport
    {
        public int Index { get; set; }
        public string SellerTin { get; set; }
        public string BuyerName { get; set; }
        public string BuyerTin { get; set; }
        public string ContractNumber { get; set; }
        public string ContractDate { get; set; }
        public string SupplyNumber { get; set; }
        public string SupplyDate { get; set; }
        public string SupplyType { get; set; }
        public double SupplyAmount { get; set; }
        public double SupplyAmountNds { get; set; }
        public string SupplyAmountRus { get; set; }
        public string SupplyAmountRusNds { get; set; }
        public string SupplyDelayDate { get; set; }
        public double DiscountedAmount { get; set; }
        public double DiscountedAmountNds20 { get; set; }
        public string DiscountedAmountRus { get; set; }
        public string DiscountedAmountRusNds20 { get; set; }
        public double DiscountPercentage { get; set; }
        public string DiscountPercentageRus { get; set; }
        public double DiscountPayment { get; set; }
        public string DiscountPaymentRus { get; set; }
    }
}