using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Discounting.API.Common.Constants;
using Discounting.API.Common.ViewModels;
using Refit;

namespace Discounting.Tests.ClientApi
{
    public interface IContractApi
    {
        [Get("/api/contracts")]
        Task<List<ContractDTO>> Get(
            int offset = Filters.Offset,
            int limit = Filters.Limit);

        [Get("/api/contracts/{id}")]
        Task<ContractDTO> Get(Guid id);

        [Post("/api/contracts")]
        Task<ContractDTO> Post(ContractDTO dto);

        [Put("/api/contracts/{id}")]
        Task<ContractDTO> Put(Guid id, ContractDTO dto);
    }
}