using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Discounting.API.Common.GridExtensions;
using Discounting.API.Common.ViewModels.AccessControl;
using Discounting.API.Common.ViewModels.Account;
using Discounting.API.Common.ViewModels.Common;
using Microsoft.AspNetCore.Mvc;
using Refit;

namespace Discounting.Tests.ClientApi.AccessControl
{
    public interface IUserApi
    {
        [Get("/api/users")]
        Task<List<UserDTO>> GetAll(QueryObjectDTO queryObject = null);

        [Get("/api/users/{id}")]
        Task<UserDTO> GetOne(Guid id);

        [Post("/api/users")]
        Task<UserDTO> Create(UserDTO dto);

        [Put("/api/users/{id}")]
        Task<UserDTO> Update(Guid id, UserDTO dto);

        [Delete("/api/users/{id}")]
        Task<ActionResult<bool>> Delete(Guid id);

        [Get("/api/users/{id}/roles")]
        Task<List<RoleDTO>> GetRoles(Guid id);

        [Post("/api/users/{id}/user-roles")]
        Task<List<UserRoleDTO>> SetRoles(Guid id, Guid[] roleIds);

        [Post("/api/users/{id}/password")]
        Task<NoContentResult> ChangePassword(Guid id, PasswordDTO password);
        
        [Post("/api/users/{id}/deactivate")]
        Task Deactivate(Guid id,  DeactivationDTO dto);
        
        [Post("/api/users/{id}/activate")]
        Task Activate(Guid id);
    }
}