using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Discounting.API.Common.Constants;
using Discounting.API.Common.ViewModels;
using Microsoft.AspNetCore.Http;
using Refit;

namespace Discounting.Tests.ClientApi
{
    public interface ISupplyApi
    {
        [Get("/api/supplies/in-process")]
        Task<List<SupplyDTO>> GetInProcess(
            int offset = Filters.Offset,
            int limit = Filters.Limit);

        [Get("/api/supplies/in-finance")]
        Task<List<SupplyDTO>> GetInFinance(
            int offset = Filters.Offset,
            int limit = Filters.Limit);

        [Get("/api/supplies/not-available")]
        Task<List<SupplyDTO>> GetNotAvailable(
            int offset = Filters.Offset,
            int limit = Filters.Limit);

        [Get("/api/supplies/{id}")]
        Task<SupplyDTO> Get(Guid id);

        [Post("/api/supplies")]
        Task<SupplyResponseDTO> Post(List<SupplyDTO> model);

        [Post("/api/excel")]
        Task<SupplyDTO> ParseExcelAsync(IFormFile file);

        [Post("/api/supplies/verify-seller-manually")]
        Task<SupplyResponseDTO> VerifySellerManually(SupplyVerificationRequestDTO model);

        [Post("/api/supplies/verify-buyer-manually")]
        Task<SupplyResponseDTO> VerifyBuyerManually(Guid[] supplyIds);
    }
}