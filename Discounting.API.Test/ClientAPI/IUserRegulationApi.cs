using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Discounting.API.Common.ViewModels.Regulations;
using Refit;

namespace Discounting.Tests.ClientApi
{
    public interface IUserRegulationApi
    {
        [Get("/api/user-regulations")]
        Task<List<UserRegulationDTO>> Get(Guid? userId = null);

        [Get("/api/user-regulations/{id}")]
        Task<UserRegulationDTO> Get(Guid id);

        [Post("/api/user-regulations")]
        Task<UserRegulationDTO> Post(UserRegulationDTO dto);

        [Put("/api/user-regulations/{id}/profile")]
        Task<UserProfileRegulationInfoDTO> Put(Guid id, UserProfileRegulationInfoDTO dto);

        [Delete("/api/user-regulations/{id}")]
        Task Delete(Guid id);
    }
}