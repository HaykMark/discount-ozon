using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Discounting.API.Common.ViewModels.TariffDiscounting;
using Refit;

namespace Discounting.Tests.ClientAPI.TariffDiscount
{
    public interface IDiscountSettingsApi
    {
        [Get("/api/discount-settings")]
        Task<List<DiscountSettingsDTO>> Get(bool onlyCurrent);

        [Get("/api/discount-settings/{id}")]
        Task<DiscountSettingsDTO> Get(Guid id);

        [Post("/api/discount-settings")]
        Task<DiscountSettingsDTO> Post(DiscountSettingsDTO dto);
        
        [Put("/api/discount-settings/{id}")]
        Task<DiscountSettingsDTO> Put(Guid id, DiscountSettingsDTO dto);
    }
}