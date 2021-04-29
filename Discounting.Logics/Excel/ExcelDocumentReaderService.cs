using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using ClosedXML.Report;
using Discounting.Common.Validation;
using Discounting.Common.Validation.Errors;
using Discounting.Entities;
using Discounting.Entities.CompanyAggregates;
using Discounting.Entities.Extensions;
using OfficeOpenXml;

namespace Discounting.Logics.Excel
{
    public interface IExcelDocumentReaderService
    {
        (List<Supply>, ValidationErrors, int) ParseExcelToSupplies(Stream stream);
    }

    public class ExcelDocumentReaderService : IExcelDocumentReaderService
    {
        public (List<Supply>, ValidationErrors, int) ParseExcelToSupplies(Stream stream)
        {
            using var package = new ExcelPackage(stream);
            var workSheet = package.Workbook.Worksheets.First();
            var totalRows = workSheet.Dimension.Rows;

            var supplies = new List<Supply>();
            var validationErrors = new ValidationErrors();

            for (var i = 2; i <= totalRows; i++)
            {
                var currentErrors = new ValidationErrors();
                currentErrors.AddRange(ValidateRequiredSupplyColumns(workSheet, i));
                if (currentErrors.Count > 0)
                {
                    validationErrors.AddRange(currentErrors);
                    continue;
                }

                var newSupply = ParseSupplyRow(workSheet, i, out var failedColumnName);
                if (newSupply == null)
                {
                    validationErrors.Add(new ValidationError(failedColumnName, new InvalidErrorDetails()));
                }
                else if (newSupply.Type != SupplyType.Ukd && newSupply.Amount < 0)
                {
                    validationErrors.Add(new ValidationError(nameof(Supply.Amount), "Amount is negative"));
                }
                else
                {
                    supplies.Add(newSupply);
                }
            }

            return (supplies, validationErrors, totalRows - 1 - supplies.Count);
        }

        private Supply ParseSupplyRow(ExcelWorksheet workSheet, int row, out string failedColumnName)
        {
            failedColumnName = null;

            var supply = new Supply
            {
                Id = Guid.NewGuid(),
                CreationDate = DateTime.UtcNow,
                Type = SupplyTypeExtension.GetType(workSheet.Cells[row, 5].Value.ToString().Trim())
            };

            if (!(workSheet.Cells[row, 7].Value is DateTime date))
            {
                failedColumnName = nameof(Supply.Date);
                return null;
            }

            if (!(workSheet.Cells[row, 4].Value is DateTime contractDate))
            {
                failedColumnName = nameof(Supply.Date);
                return null;
            }

            if (!decimal.TryParse(workSheet.Cells[row, 8].Value
                    .ToString()
                    .Trim()
                    .Replace(',', '.'),
                NumberStyles.Number,
                CultureInfo.InvariantCulture,
                out var amount))
            {
                failedColumnName = nameof(Supply.Amount);
                return null;
            }

            if (!(workSheet.Cells[row, 9].Value is DateTime delayEndDate))
            {
                failedColumnName = nameof(Supply.DelayEndDate);
                return null;
            }

            if (!Supply.IsMainType(supply.Type))
            {
                if (!(workSheet.Cells[row, 12].Value is DateTime baseDate))
                {
                    failedColumnName = nameof(Supply.BaseDocumentDate);
                    return null;
                }

                supply.BaseDocumentType = SupplyTypeExtension.GetType(workSheet.Cells[row, 10].Value.ToString().Trim());
                supply.BaseDocumentNumber = workSheet.Cells[row, 11].Value.ToString().Trim();
                supply.BaseDocumentDate = baseDate;
            }

            supply.Contract = new Contract()
            {
                Provider = ContractProvider.Automatically,
                Seller = new Company
                {
                    TIN = workSheet.Cells[row, 1].Value.ToString().Trim(),
                    CompanyType = CompanyType.SellerBuyer
                },
                Buyer = new Company
                {
                    TIN = workSheet.Cells[row, 2].Value.ToString().Trim(),
                    CompanyType = CompanyType.SellerBuyer
                }
            };

            supply.Number = workSheet.Cells[row, 6].Value.ToString().Trim();
            supply.Date = date;
            supply.Amount = amount;
            supply.DelayEndDate = delayEndDate;
            supply.ContractNumber = workSheet.Cells[row, 3].Value.ToString().Trim();
            ;
            supply.ContractDate = contractDate;
            return supply;
        }

        private IEnumerable<ValidationError> ValidateRequiredSupplyColumns(ExcelWorksheet workSheet, int row)
        {
            var mainType = SupplyType.None;

            if (workSheet.Cells[row, 1].Value == null ||
                string.IsNullOrWhiteSpace(workSheet.Cells[row, 1].Value.ToString().Trim()))
            {
                yield return new RequiredFieldValidationError("SellerTin");
            }

            if (workSheet.Cells[row, 2].Value == null ||
                string.IsNullOrWhiteSpace(workSheet.Cells[row, 2].Value.ToString().Trim()))
            {
                yield return new RequiredFieldValidationError("BuyerTin");
            }

            if (workSheet.Cells[row, 3].Value == null ||
                string.IsNullOrWhiteSpace(workSheet.Cells[row, 3].Value.ToString().Trim()))
            {
                yield return new RequiredFieldValidationError(nameof(Supply.ContractNumber));
            }

            if (workSheet.Cells[row, 4].Value == null ||
                string.IsNullOrWhiteSpace(workSheet.Cells[row, 4].Value.ToString().Trim()))
            {
                yield return new RequiredFieldValidationError(nameof(Supply.ContractDate));
            }

            if (workSheet.Cells[row, 5].Value == null ||
                SupplyTypeExtension.GetType(workSheet.Cells[row, 5].Value.ToString().Trim()) == SupplyType.None)
            {
                yield return new RequiredFieldValidationError(nameof(Supply.Type));
            }
            else
            {
                mainType = SupplyTypeExtension.GetType(workSheet.Cells[row, 5].Value.ToString().Trim());
            }

            if (workSheet.Cells[row, 6].Value == null ||
                string.IsNullOrWhiteSpace(workSheet.Cells[row, 6].Value.ToString().Trim()))
            {
                yield return new RequiredFieldValidationError(nameof(Supply.Number));
            }

            if (workSheet.Cells[row, 7].Value == null ||
                string.IsNullOrWhiteSpace(workSheet.Cells[row, 7].Value.ToString().Trim()))
            {
                yield return new RequiredFieldValidationError(nameof(Supply.Date));
            }

            if (workSheet.Cells[row, 8].Value == null ||
                string.IsNullOrWhiteSpace(workSheet.Cells[row, 8].Value.ToString().Trim()))
            {
                yield return new RequiredFieldValidationError(nameof(Supply.Amount));
            }

            if (workSheet.Cells[row, 9].Value == null ||
                string.IsNullOrWhiteSpace(workSheet.Cells[row, 9].Value.ToString().Trim()))
            {
                yield return new RequiredFieldValidationError(nameof(Supply.DelayEndDate));
            }

            if (mainType != SupplyType.None &&
                !Supply.IsMainType(mainType))
            {
                if (workSheet.Cells[row, 10].Value == null ||
                    SupplyTypeExtension.GetType(workSheet.Cells[row, 10].Value.ToString().Trim()) == SupplyType.None)
                {
                    yield return new RequiredFieldValidationError(nameof(Supply.BaseDocumentType));
                }

                if (workSheet.Cells[row, 11].Value == null ||
                    string.IsNullOrWhiteSpace(workSheet.Cells[row, 11].Value.ToString().Trim()))
                {
                    yield return new RequiredFieldValidationError(nameof(Supply.BaseDocumentNumber));
                }

                if (workSheet.Cells[row, 12].Value == null ||
                    string.IsNullOrWhiteSpace(workSheet.Cells[row, 12].Value.ToString().Trim()))
                {
                    yield return new RequiredFieldValidationError(nameof(Supply.BaseDocumentDate));
                }
            }
        }
    }
}