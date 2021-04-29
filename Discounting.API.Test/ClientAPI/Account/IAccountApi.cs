using System.Threading.Tasks;
using Discounting.API.Common.Response;
using Discounting.API.Common.ViewModels.Account;
using Microsoft.AspNetCore.Mvc;
using Refit;

namespace Discounting.Tests.ClientApi.Account
{
    public interface IAccountApi
    {
        [Post("/api/user-account/login")]
        Task<SessionInfoDTO> Login(LoginDTO dto);

        [Post("/api/user-account/register")]
        Task<SessionInfoDTO> Register(RegistrationDTO dto);

        [Post("/api/user-account/login")]
        Task<DataResponseBody<SessionInfoDTO>> LoginWithDataResponseBody(LoginDTO dto);

        [Post("/api/user-account/token/refresh")]
        Task<SessionInfoDTO> RefreshToken(TokenDTO token);

        [Post("/api/user-account/logout")]
        Task Logout(TokenDTO token);

        [Get("/api/user-account")]
        Task<UserDTO> GetUserInfo();

        [Post("/api/user-account/password/change")]
        Task<IActionResult> ChangePassword(ChangePasswordDTO model);
    }
}