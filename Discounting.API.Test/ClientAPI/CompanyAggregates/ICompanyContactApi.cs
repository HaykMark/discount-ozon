using System;
using System.Threading.Tasks;
using Discounting.API.Common.Constants;
using Discounting.API.Common.ViewModels.Company;
using Refit;

namespace Discounting.Tests.ClientAPI.CompanyAggregates
{
    public interface ICompanyContactApi
    {
        [Get("/api/companies/{id}/contact")]
        Task<CompanyContactInfoDTO> Get(
            Guid id,
            int offset = Filters.Offset,
            int limit = Filters.Limit
        );

        [Post("/api/companies/{id}/contact")]
        Task<CompanyContactInfoDTO> Create(Guid id, CompanyContactInfoDTO dto);

        [Put("/api/companies/{id}/contact/{sid}")]
        Task<CompanyContactInfoDTO> Update(Guid id, Guid sid, CompanyContactInfoDTO dto);
    }
}