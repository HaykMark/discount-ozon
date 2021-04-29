using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Discounting.API.Common.Constants;
using Discounting.API.Common.ViewModels.TariffDiscounting;
using Refit;

namespace Discounting.Tests.ClientAPI.TariffDiscount
{
    public interface ITariffApi
    {
        [Get("/api/tariffs")]
        Task<List<TariffDTO>> GetAll(
            int offset = Filters.Offset,
            int limit = Filters.Limit,
            Guid? companyId = null);

        [Get("/api/tariffs/{id}")]
        Task<TariffDTO> Get(Guid id);

        [Post("/api/tariffs")]
        Task<List<TariffDTO>> Post(List<TariffDTO> model);

        [Get("/api/tariffs/archives")]
        Task<List<TariffArchiveDTO>> GetAllArchives(
            int offset = Filters.Offset,
            int limit = Filters.Limit,
            Guid? companyId = null);
    }
}