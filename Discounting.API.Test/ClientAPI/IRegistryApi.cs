using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Discounting.API.Common.Constants;
using Discounting.API.Common.ViewModels;
using Discounting.API.Common.ViewModels.Registry;
using Discounting.API.Common.ViewModels.TariffDiscounting;
using Discounting.Common.Response;
using Discounting.Entities;
using Discounting.Entities.Templates;
using Microsoft.AspNetCore.Mvc;
using Refit;

namespace Discounting.Tests.ClientApi
{
    public interface IRegistryApi
    {
        [Get("/api/registries/in-process")]
        Task<List<RegistryDTO>> GetInProcess(
            int offset = Filters.Offset,
            int limit = Filters.Limit);

        [Get("/api/registries/finished")]
        Task<List<RegistryDTO>> GetFinished(
            int offset = Filters.Offset,
            int limit = Filters.Limit);

        [Get("/api/registries/declined")]
        Task<List<RegistryDTO>> GetDeclined(
            int offset = Filters.Offset,
            int limit = Filters.Limit);

        [Get("/api/registries/{id}")]
        Task<RegistryDTO> Get(Guid id);

        [Post("/api/registries")]
        Task<RegistryDTO> Post(RegistryRequestDTO dto);

        [Put("/api/registries/{id}")]
        Task<RegistryDTO> Put(Guid id, RegistryDTO dto);

        [Get("/api/registries/{id}/supplies")]
        Task<List<SupplyDTO>> GetSupplies(Guid id,
            int offset = Filters.Offset,
            int limit = Filters.Limit);

        [Post("/api/registries/{id}/set-supplies")]
        Task<RegistryDTO> SetSupplies(Guid id, Guid[] supplyIds);

        [Get("/api/registries/{id}/discount")]
        Task<DiscountDTO> GetDiscount(Guid id);

        [Get("/api/registries/{id}/supply-discounts")]
        Task<ResourceCollection<SupplyDiscountDTO>> GetSupplyDiscounts(Guid id);

        [Get("/api/registries/{id}/file")]
        Task<PhysicalFileResult> GetFile(Guid id, TemplateType type);
    }
}