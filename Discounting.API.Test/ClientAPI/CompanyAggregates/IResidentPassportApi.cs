using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Discounting.API.Common.Constants;
using Discounting.API.Common.ViewModels.Company;
using Refit;

namespace Discounting.Tests.ClientAPI.CompanyAggregates
{
    public interface IResidentPassportApi
    {
        [Get("/api/companies/{id}/resident-passports")]
        Task<List<ResidentPassportInfoDTO>> Get(
            Guid id,
            int offset = Filters.Offset,
            int limit = Filters.Limit
        );

        [Post("/api/companies/{id}/resident-passports")]
        Task<ResidentPassportInfoDTO> Create(Guid id, ResidentPassportInfoDTO dto);

        [Put("/api/companies/{id}/resident-passports/{sid}")]
        Task<ResidentPassportInfoDTO> Update(Guid id, Guid sid, ResidentPassportInfoDTO dto);
    }
}