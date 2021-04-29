using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Discounting.API.Common.ViewModels.AccessControl;
using Discounting.API.Common.ViewModels.Account;
using Discounting.Common.Response;
using Microsoft.AspNetCore.Mvc;
using Refit;

namespace Discounting.Tests.ClientApi.AccessControl
{
    public interface IRoleApi
    {
        [Get("/api/roles")]
        Task<List<RoleDTO>> GetAll();

        [Get("/api/roles/{id}")]
        Task<RoleDTO> GetOne(Guid id);

        [Post("/api/roles")]
        Task<RoleDTO> Create(RoleDTO dto);

        [Patch("/api/roles")]
        Task<ResourceCollection<RoleDTO>> UpdateRange(List<RoleDTO> dto);

        [Put("/api/roles/{id}")]
        Task<RoleDTO> Update(Guid id, RoleDTO dto);

        [Delete("/api/roles/{id}")]
        Task<ApiResponse<NoContentResult>> Delete(Guid id);

        [Post("/api/roles/{id}/user-roles")]
        Task<ResourceCollection<UserRoleDTO>> SetUserRoles(Guid id, Guid[] userIds);

        [Get("/api/roles/{id}/users")]
        Task<ResourceCollection<UserDTO>> GetUsers(Guid id);

        [Get("/api/roles/{id}/user-roles")]
        Task<ResourceCollection<UserRoleDTO>> GetUserRoles(Guid id);
    }
}