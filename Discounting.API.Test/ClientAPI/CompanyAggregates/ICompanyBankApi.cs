using System;
using System.Threading.Tasks;
using Discounting.API.Common.ViewModels.Company;
using Refit;

namespace Discounting.Tests.ClientAPI.CompanyAggregates
{
    public interface ICompanyBankApi
    {
        [Get("/api/companies/{id}/bank")]
        Task<CompanyBankInfoDTO> Get(Guid id
        );

        [Post("/api/companies/{id}/bank")]
        Task<CompanyBankInfoDTO> Create(Guid id, CompanyBankInfoDTO dto);
    }
}