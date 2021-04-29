using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Discounting.API.Common.Constants;
using Discounting.API.Common.ViewModels.TariffDiscounting;
using Refit;

namespace Discounting.Tests.ClientAPI.TariffDiscount
{
    public interface IDiscountApi
    {
        [Get("/api/discounts")]
        Task<List<DiscountDTO>> Get(
            int offset = Filters.Offset,
            int limit = Filters.Limit);

        [Get("/api/discounts/{id}")]
        Task<DiscountDTO> Get(Guid id);

        [Post("/api/discounts")]
        Task<DiscountDTO> Post(DiscountDTO dto);

        [Put("/api/discounts/{id}")]
        Task<DiscountDTO> Put(Guid id, DiscountDTO dto);

        [Post("/api/discounts/{id}/supplies-discounts")]
        Task<List<SupplyDiscountDTO>> CreateSupplyDiscount(Guid id, SupplyDiscountDTO[] supplyDiscountDtos);
    }
}