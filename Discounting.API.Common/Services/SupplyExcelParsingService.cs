using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Threading.Tasks;
using AutoMapper;
using Discounting.API.Common.CustomAttributes;
using Discounting.API.Common.ViewModels;
using Discounting.Common.Exceptions;
using Discounting.Common.Validation;
using Discounting.Entities;
using Discounting.Logics;
using Discounting.Logics.Excel;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore.Internal;

namespace Discounting.API.Common.Services
{
    public interface ISupplyExcelParsingService
    {
        Task<SupplyResponseDTO> TryParseExcelAsync(IFormFile file);
    }

    public class SupplyExcelParsingService : ISupplyExcelParsingService
    {
        private readonly IMapper mapper;
        private readonly ISupplyService supplyService;
        private readonly IExcelDocumentReaderService excelDocumentReaderService;

        public SupplyExcelParsingService
        (
            IExcelDocumentReaderService excelDocumentReaderService,
            ISupplyService supplyService,
            IMapper mapper
        )
        {
            this.excelDocumentReaderService = excelDocumentReaderService;
            this.supplyService = supplyService;
            this.mapper = mapper;
        }

        public async Task<SupplyResponseDTO> TryParseExcelAsync(IFormFile file)
        {
            if (file.ContentType != "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
            {
                throw new ForbiddenException("wrong-excel-content-type", "Wrong excel content type.");
            }
            var supplies = new List<Supply>();
            var validationErrors = new ValidationErrors();
            var failedCount = 0;
            await using (var stream = file.OpenReadStream())
            {
                var (parsedSupplies, excelErrors, failedRowsCount) =
                    excelDocumentReaderService.ParseExcelToSupplies(stream);
                supplies.AddRange(parsedSupplies);
                validationErrors.AddRange(excelErrors);
                failedCount += failedRowsCount;
            }

            if (!supplies.Any())
            {
                return new SupplyResponseDTO
                {
                    Supplies = new List<SupplyDTO>(),
                    Errors = validationErrors,
                    ImportedCount = 0,
                    FailedCount = failedCount
                };
            }
            var (addedSupplies, errors) = await supplyService.CreateAsync(supplies, SupplyProvider.Manually);
            var supplyDtos = mapper.Map<List<SupplyDTO>>(addedSupplies);
            validationErrors.AddRange(errors);
            failedCount += supplies.Count - addedSupplies.Count;

            return new SupplyResponseDTO
            {
                Supplies = supplyDtos,
                Errors = validationErrors,
                ImportedCount = addedSupplies.Count,
                FailedCount = failedCount
            };
        }
    }
}