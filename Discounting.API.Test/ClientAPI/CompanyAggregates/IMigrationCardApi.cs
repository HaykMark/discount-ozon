using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Discounting.API.Common.Constants;
using Discounting.API.Common.ViewModels.Company;
using Refit;

namespace Discounting.Tests.ClientAPI.CompanyAggregates
{
    public interface IMigrationCardDataApi
    {
        [Get("/api/companies/{id}/migration-cards")]
        Task<List<MigrationCardInfoDTO>> Get(
            Guid id,
            int offset = Filters.Offset,
            int limit = Filters.Limit
        );

        [Post("/api/companies/{id}/migration-cards")]
        Task<MigrationCardInfoDTO> Create(Guid id, MigrationCardInfoDTO dto);

        [Put("/api/companies/{id}/migration-cards/{sid}")]
        Task<MigrationCardInfoDTO> Update(Guid id, Guid sid, MigrationCardInfoDTO dto);
    }
}