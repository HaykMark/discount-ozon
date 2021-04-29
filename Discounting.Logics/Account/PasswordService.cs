using System;
using System.Threading.Tasks;
using Discounting.Common.Exceptions;
using Discounting.Common.Helpers;
using Discounting.Common.Validation.Errors;
using Discounting.Data.Context;
using Discounting.Entities.Account;

namespace Discounting.Logics.Account
{
   public interface IPasswordService
    {
        Task<User> ChangePasswordAsync(Guid userId, string password);

        Task<User> ChangePasswordAsync(string currentPassword, string newPassword);

        Task<User> ResetPasswordAsync(
            Guid userId,
            string newPassword,
            string confirmationCode,
            int activationTokenExpirationDuration
        );

        Task<bool> IsValidUserConfirmationCodeAsync(
            Guid userId,
            string confirmationCode,
            int activationTokenExpirationDuration);
    }

    public class PasswordService : IPasswordService
    {
        private readonly IUserService userService;
        private readonly IUnitOfWork unitOfWork;
        private readonly IAuthenticationService authenticationService;

        public PasswordService(
            IUnitOfWork unitOfWork,
            IUserService userService,
            IAuthenticationService authenticationService
        )
        {
            this.unitOfWork = unitOfWork;
            this.userService = userService;
            this.authenticationService = authenticationService;
        }

        public async Task<User> ChangePasswordAsync(string currentPassword, string newPassword)
        {
            var user = await userService.GetCurrentUserAsync();
            var currentPasswordHash = SecurityToolkit.GetSha512Hash(currentPassword, user.Salt);
            if (user.Password != currentPasswordHash)
            {
                throw new ValidationException(nameof(User.Password),
                    "Current password is wrong.",
                    new GeneralErrorDetails("wrong-password"));
            }

            if (currentPassword == newPassword)
            {
                throw new ValidationException(nameof(User.Password),
                    "Current password is the same as the old one.",
                    new GeneralErrorDetails("same-password"));
            }

            await ChangePasswordAsync(user.Id, newPassword);
            return user;
        }

        public async Task<User> ChangePasswordAsync(Guid userId, string password)
        {
            var user = await unitOfWork.GetOrFailAsync<User, Guid>(userId);
            var passwordHash = SecurityToolkit.GetSha512Hash(password, user.Salt);
            if (!string.IsNullOrWhiteSpace(user.Password) && user.Password == passwordHash)
            {
                throw new ValidationException(nameof(User.Password),
                    "Current password is the same as the old one.",
                    new GeneralErrorDetails("same-password"));
            }
            user.Salt = SecurityToolkit.GetSalt();
            user.Password = SecurityToolkit.GetSha512Hash(password, user.Salt);
            user.SerialNumber = Guid.NewGuid().ToString("N"); // expire other logins.
            await unitOfWork.UpdateAndSaveAsync<User, Guid>(user);
            return user;
        }

        public async Task<User> ResetPasswordAsync(
            Guid userId,
            string newPassword,
            string confirmationCode,
            int activationTokenExpirationDuration
        )
        {
            try
            {
                var user = await unitOfWork.GetOrFailAsync<User, Guid>(userId);
                if (!authenticationService.IsValidEmailConfirmationCode(
                    user,
                    confirmationCode,
                    activationTokenExpirationDuration
                ))
                {
                    throw new ForbiddenException();
                }

                user.Salt = SecurityToolkit.GetSalt();
                user.Password = SecurityToolkit.GetSha512Hash(newPassword, user.Salt);
                user.IsActive = true;
                user.IsEmailConfirmed = true;
                user.SerialNumber = Guid.NewGuid().ToString("N"); // expire other logins.
                user.ActivationToken = null;
                user.ActivationTokenCreationDateTime = null;

                //There is not current user
                await unitOfWork.UpdateAndSaveAsync<User, Guid>(user);
                return user;
            }
            catch (NotFoundException)
            {
                throw new ForbiddenException();
            }
        }


        public async Task<bool> IsValidUserConfirmationCodeAsync(
            Guid userId,
            string confirmationCode,
            int activationTokenExpirationDuration
        )
        {
            try
            {
                var user = await unitOfWork.GetOrFailAsync<User, Guid>(userId);
                return authenticationService.IsValidEmailConfirmationCode(
                    user,
                    confirmationCode,
                    activationTokenExpirationDuration
                );
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}