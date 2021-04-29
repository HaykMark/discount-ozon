using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Discounting.API.Common.Constants;
using Discounting.API.Common.ViewModels.Templates;
using Microsoft.AspNetCore.Mvc;
using Refit;

namespace Discounting.Tests.ClientApi
{
    public interface IBuyerTemplateConnectionApi
    {
        [Get("/api/buyer-template-connections")]
        Task<List<BuyerTemplateConnectionDTO>> Get(
            int offset = Filters.Offset,
            int limit = Filters.Limit);

        [Get("/api/buyer-template-connections/{id}")]
        Task<BuyerTemplateConnectionDTO> Get(Guid id);

        [Post("/api/buyer-template-connections")]
        Task<BuyerTemplateConnectionDTO> Post(BuyerTemplateConnectionDTO dto);

        [Put("/api/buyer-template-connections/{id}")]
        Task<BuyerTemplateConnectionDTO> Put(Guid id, BuyerTemplateConnectionDTO dto);

        [Delete("/api/buyer-template-connections/{id}")]
        Task<ApiResponse<NoContentResult>> Delete(Guid id);
    }
}