using System;
using System.Threading.Tasks;
using Discounting.API.Common.Constants;
using Discounting.API.Common.ViewModels.Company;
using Refit;

namespace Discounting.Tests.ClientAPI.CompanyAggregates
{
    public interface ICompanyAuthorizedUserApi
    {
        [Get("/api/companies/{id}/authorized-user")]
        Task<CompanyAuthorizedUserInfoDTO> Get(
            Guid id,
            int offset = Filters.Offset,
            int limit = Filters.Limit
        );

        [Post("/api/companies/{id}/authorized-user")]
        Task<CompanyAuthorizedUserInfoDTO> Create(Guid id, CompanyAuthorizedUserInfoDTO dto);

        [Put("/api/companies/{id}/authorized-user/{sid}")]
        Task<CompanyAuthorizedUserInfoDTO> Update(Guid id, Guid sid, CompanyAuthorizedUserInfoDTO dto);
    }
}