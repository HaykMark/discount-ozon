using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Discounting.API.Common.Constants;
using Discounting.API.Common.ViewModels;
using Discounting.API.Common.ViewModels.Common;
using Discounting.API.Common.ViewModels.Company;
using Discounting.API.Common.ViewModels.TariffDiscounting;
using Discounting.Common.Response;
using Discounting.Entities.Account;
using Refit;

namespace Discounting.Tests.ClientAPI.CompanyAggregates
{
    public interface ICompanyApi
    {
        [Get("/api/companies")]
        Task<List<CompanyDTO>> Get(
            int offset = Filters.Offset,
            int limit = Filters.Limit,
            string tin = null);

        [Get("/api/companies/{id}")]
        Task<CompanyDTO> Get(Guid id);

        [Post("/api/companies")]
        Task<CompanyDTO> Post(CompanyDTO dto);

        [Put("/api/companies/{id}")]
        Task<CompanyDTO> Put(Guid id, CompanyDTO dto);


        [Get("/api/companies/{id}/users")]
        Task<ResourceCollection<User>> GetUsers(
            Guid id,
            int offset = Filters.Offset,
            int limit = Filters.Limit
        );

        [Get("/api/companies/{id}/contracts")]
        Task<ResourceCollection<ContractDTO>> GetContracts(
            Guid id,
            int offset = Filters.Offset,
            int limit = Filters.Limit
        );

        [Get("/api/companies/{id}/tariffs")]
        Task<ResourceCollection<TariffDTO>> GetTariffs(
            Guid id,
            int offset = Filters.Offset,
            int limit = Filters.Limit
        );

        [Get("/api/companies/{id}/settings")]
        Task<CompanySettingsDTO> GetSettings(Guid id);

        [Post("/api/companies/{id}/settings")]
        Task<CompanySettingsDTO> CreateSettings(Guid id, CompanySettingsDTO dto);

        [Put("/api/companies/{id}/settings/{sid}")]
        Task<CompanySettingsDTO> UpdateSettings(Guid id, Guid sid, CompanySettingsDTO dto);
        
        [Post("/api/companies/{id}/deactivate")]
        Task Deactivate(Guid id,  DeactivationDTO dto);
        
        [Post("/api/companies/{id}/activate")]
        Task Activate(Guid id);
    }
}