using System;
using System.Threading.Tasks;
using Discounting.API.Common.Constants;
using Discounting.API.Common.ViewModels.Company;
using Discounting.Common.Response;
using Refit;

namespace Discounting.Tests.ClientApi
{
    public interface IFactoringAgreementApi
    {
        [Get("/api/factoring-agreements")]
        Task<ResourceCollection<FactoringAgreementDTO>> Get(
            Guid? companyId = null,
            string supplyNumber = null,
            int offset = Filters.Offset,
            int limit = Filters.Limit
        );

        [Post("/api/factoring-agreements")]
        Task<FactoringAgreementDTO> Create(FactoringAgreementDTO dto);

        [Put("/api/factoring-agreements/{id}")]
        Task<FactoringAgreementDTO> Update(Guid id, FactoringAgreementDTO dto);
    }
}