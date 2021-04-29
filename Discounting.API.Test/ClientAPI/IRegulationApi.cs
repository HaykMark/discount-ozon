using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Discounting.API.Common.ViewModels;
using Discounting.API.Common.ViewModels.Regulations;
using Discounting.Entities;
using Discounting.Entities.Regulations;
using Refit;

namespace Discounting.Tests.ClientApi
{
    public interface IRegulationApi
    {
        [Get("/api/regulations")]
        Task<List<RegulationDTO>> Get(RegulationType? type = null);

        [Get("/api/regulations/{id}")]
        Task<RegulationDTO> Get(Guid id);

        [Post("/api/regulations")]
        Task<RegulationDTO> Post(RegulationDTO dto);

        [Put("/api/regulations/{id}")]
        Task<RegulationDTO> Put(Guid id, RegulationDTO dto);
    }
}