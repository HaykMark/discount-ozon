using System;
using System.Threading.Tasks;
using Discounting.API.Common.Constants;
using Discounting.API.Common.ViewModels.Company;
using Refit;

namespace Discounting.Tests.ClientAPI.CompanyAggregates
{
    public interface ICompanyOwnerPositionApi
    {
        [Get("/api/companies/{id}/owner-position")]
        Task<CompanyOwnerPositionInfoDTO> Get(
            Guid id,
            int offset = Filters.Offset,
            int limit = Filters.Limit
        );

        [Post("/api/companies/{id}/owner-position")]
        Task<CompanyOwnerPositionInfoDTO> Create(Guid id, CompanyOwnerPositionInfoDTO dto);

        [Put("/api/companies/{id}/owner-position/{sid}")]
        Task<CompanyOwnerPositionInfoDTO> Update(Guid id, Guid sid, CompanyOwnerPositionInfoDTO dto);
    }
}