using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discounting.Common.Exceptions;
using Discounting.Common.Helpers;
using Discounting.Data.Context;
using Discounting.Entities;
using Discounting.Entities.Account;
using Discounting.Entities.CompanyAggregates;
using Discounting.Entities.Regulations;
using Discounting.Logics.Validators.Account;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Discounting.Logics.Account
{
    public interface IUserService
    {
        Task<(User[], int)> GetAll(int offset, int limit);
        Task<User> FindUserByEmailAsync(string email);

        Task<User> FindUserByEmailAsync(string email, string password, int maxPasswordRetryLimit);

        Task<User> CreateUserAsync(User user, int maxPasswordRetryCount);

        Task<User> CreateUserAsync(User user, string password, int maxPasswordRetryCount);

        Task<User> UpdateUserAsync(User user);

        Task UpdateUserLastActivityDateAsync(Guid userId);

        Task<User> GetCurrentUserAsync();

        Task<User> GenerateEmailConfirmationCode(User user, string baseUrl);

        Task<User> EmailActivationAsync(User user);

        Task RemoveAsync(Guid userId);
        Task DeactivateAsync(Guid id, string deactivationReason);
        Task ActivateAsync(Guid id, int maxPasswordRetryLimit);
    }

    public class UserService : IUserService
    {
        private readonly DbSet<User> users;
        private readonly IUnitOfWork unitOfWork;
        private readonly ISessionService sessionService;
        private readonly IUserValidator userValidator;
        private readonly ILogger<UserService> logger;

        public UserService(
            IUnitOfWork unitOfWork,
            ISessionService sessionService,
            IUserValidator userValidator,
            ILogger<UserService> logger
        )
        {
            users = unitOfWork.Set<User>();
            this.sessionService = sessionService;
            this.userValidator = userValidator;
            this.logger = logger;
            this.unitOfWork = unitOfWork;
        }


        public async Task<(User[], int)> GetAll(int offset, int limit)
        {
            var currentUserId = sessionService.GetCurrentUserId();
            var currentUser = await unitOfWork.GetOrFailAsync<User, Guid>(currentUserId);
            return (await users
                    .Where(u => currentUser.IsSuperAdmin || u.CompanyId == currentUser.CompanyId)
                    .OrderBy(u => u.Email)
                    .Skip(offset)
                    .Take(limit)
                    .ToArrayAsync(),
                await users.CountAsync());
        }

        public async Task<User> FindUserByEmailAsync(string email)
        {
            email = email.ToLower();
            var user = await unitOfWork.GetOrFailAsync(users.Include(u => u.Company)
                .Where(u => u.Email.ToLower() == email));

            return user;
        }

        public async Task<User> FindUserByEmailAsync(string email, string password, int maxPasswordRetryLimit)
        {
            try
            {
                var user = await FindUserByEmailAsync(email);
                if (user.PasswordRetryLimit == 0)
                {
                    throw new ForbiddenException("password-retry-limit-exceeded", "User exceeded password retry limit");
                }

                var passwordHash = SecurityToolkit.GetSha512Hash(password, user.Salt);
                if (user.Password != passwordHash)
                {
                    --user.PasswordRetryLimit;
                    if (user.PasswordRetryLimit == 0)
                    {
                        user.IsActive = false;
                        user.DeactivationReason = "Password retry attempts exceeded allowed limit";
                    }

                    await unitOfWork.UpdateAndSaveAsync<User, Guid>(user);
                    throw new ForbiddenException("invalid-password", "Invalid password", user.PasswordRetryLimit.ToString());
                }
                if (user.PasswordRetryLimit != maxPasswordRetryLimit)
                {
                    user.PasswordRetryLimit = maxPasswordRetryLimit;
                    await unitOfWork.UpdateAndSaveAsync<User, Guid>(user);
                }

                return user;
            }
            catch (NotFoundException)
            {
                throw new ForbiddenException("invalid-email-or-password", "Invalid Email or Password");
            }
        }

        public async Task<User> GetCurrentUserAsync()
        {
            var userId = sessionService.GetCurrentUserId();
            if (userId == default)
                return null;
            return await unitOfWork.GetOrFailAsync<User, Guid>(userId);
        }

        public async Task<User> CreateUserAsync(User user, int maxPasswordRetryCount)
        {
            user.Password = new Guid().ToString();
            user.Salt = new Guid().ToString();
            user.SerialNumber = Guid.NewGuid().ToString("N");
            var currentUser = await GetCurrentUserAsync();
            user.CompanyId = currentUser.IsSuperAdmin
                ? user.CompanyId
                : currentUser.CompanyId;
            user.CreationDate = DateTime.UtcNow;
            user.PasswordRetryLimit = maxPasswordRetryCount;
            await userValidator.ValidateUserAsync(user, currentUser);
            return await unitOfWork.AddAndSaveAsync(user);
        }

        public async Task<User> CreateUserAsync(User user, string password, int maxPasswordRetryCount)
        {
            await userValidator.ValidateUserAsync(user);
            InitUser(user, password, maxPasswordRetryCount);
            await InitCompany(user);
            return await unitOfWork.AddAndSaveAsync(user);
        }

        public async Task<User> UpdateUserAsync(User user)
        {
            var currentUser = await GetCurrentUserAsync();
            await userValidator.ValidateUserAsync(user, currentUser);
            await ActivateCompanyIfConfirmedAsync(user);
            unitOfWork.DetachLocal(user, user.Id);
            return await unitOfWork.UpdateAndSaveAsync<User, Guid>(user);
        }

        public async Task UpdateUserLastActivityDateAsync(Guid userId)
        {
            var user = await unitOfWork.GetOrFailAsync<User, Guid>(userId);
            if (user.LastLoggedIn != null)
            {
                var updateLastActivityDate = TimeSpan.FromMinutes(2);
                var currentUtc = DateTimeOffset.UtcNow;
                var timeElapsed = currentUtc.Subtract(user.LastLoggedIn.Value);
                if (timeElapsed < updateLastActivityDate)
                {
                    return;
                }
            }

            user.LastLoggedIn = DateTimeOffset.UtcNow;
            await unitOfWork.UpdateAndSaveAsync<User, Guid>(user);
        }

        public async Task RemoveAsync(Guid userId)
        {
            var user = await unitOfWork.GetOrFailAsync<User, Guid>(userId);
            if (user.IsSuperAdmin)
            {
                throw new ForbiddenException();
            }

            await unitOfWork.RemoveAndSaveAsync<User, Guid>(userId);
        }

        public async Task<User> GenerateEmailConfirmationCode(User user, string baseUrl)
        {
            var userActivationToken = user.Id + Guid.NewGuid().ToString();

            user.ActivationToken = userActivationToken;
            user.ActivationTokenCreationDateTime = DateTime.UtcNow;
            unitOfWork.DetachLocal(user, user.Id);
            return await unitOfWork.UpdateAndSaveAsync<User, Guid>(user);
        }

        public async Task<User> EmailActivationAsync(User user)
        {
            user.IsActive = true;
            user.IsEmailConfirmed = true;
            user.ActivationToken = null;
            user.ActivationTokenCreationDateTime = null;
            var role = await unitOfWork.Set<Role>()
                .FirstAsync(r => r.Type == RoleType.InactiveCompany &&
                                 r.IsSystemDefault);
            await unitOfWork.Set<UserRole>()
                .AddAsync(new UserRole
                {
                    UserId = user.Id,
                    RoleId = role.Id
                });
            await unitOfWork.UpdateAndSaveAsync<User, Guid>(user);
            return user;
        }

        public async Task DeactivateAsync(Guid id, string deactivationReason)
        {
            var userId = sessionService.GetCurrentUserId();
            var currentUser = await unitOfWork.GetOrFailAsync<User, Guid>(userId);
            var user = await unitOfWork.GetOrFailAsync<User, Guid>(id);
            userValidator.ValidateDeactivation(user, currentUser);
            user.IsActive = false;
            user.DeactivationReason = deactivationReason;
            await unitOfWork.UpdateAndSaveAsync<User, Guid>(user);
            logger.LogInformation($"User was deactivated. UserId: {userId}");
        }

        public async Task ActivateAsync(Guid id, int maxPasswordRetryLimit)
        {
            var userId = sessionService.GetCurrentUserId();
            var currentUser = await unitOfWork.GetOrFailAsync<User, Guid>(userId);
            var user = await unitOfWork.GetOrFailAsync<User, Guid>(id);
            userValidator.ValidateActivation(user, currentUser);
            user.IsActive = true;
            user.PasswordRetryLimit = maxPasswordRetryLimit;
            await unitOfWork.UpdateAndSaveAsync<User, Guid>(user);
            logger.LogInformation($"User was reactivated. UserId: {userId}");
        }

        private async Task ActivateCompanyIfConfirmedAsync(User user)
        {
            if (user.IsConfirmedByAdmin &&
                await unitOfWork.Set<User>().AnyAsync(u => u.Id == user.Id &&
                                                           !u.IsConfirmedByAdmin))
            {
                var company = await unitOfWork.GetOrFailAsync<Company, Guid>(user.CompanyId);
                if (!company.IsActive && user.IsAdmin)
                {
                    await ValidateCompanyRegulationAsync(user.CompanyId);
                    company.IsActive = true;
                    unitOfWork.Set<Company>().Update(company);
                }
                else if (!user.IsAdmin)
                {
                    await ValidateUserRegulationAsync(user.Id, user.CompanyId);
                }

                var roleType = company.CompanyType switch
                {
                    CompanyType.SellerBuyer => RoleType.SellerBuyer,
                    CompanyType.Bank => RoleType.Bank,
                    _ => throw new ArgumentOutOfRangeException()
                };
                var role = await unitOfWork.Set<Role>().FirstAsync(r => r.Type == roleType && r.IsSystemDefault);
                var prevRoles = unitOfWork.Set<UserRole>().Where(u => u.UserId == user.Id);
                unitOfWork.Set<UserRole>().RemoveRange(prevRoles);
                await unitOfWork.Set<UserRole>().AddAsync(new UserRole
                {
                    UserId = user.Id,
                    RoleId = role.Id
                });
            }
        }

        private async Task ValidateCompanyRegulationAsync(Guid companyId)
        {
            if (!await unitOfWork.Set<CompanyRegulation>().AnyAsync(c => c.CompanyId == companyId))
            {
                throw new NotFoundException(typeof(CompanyRegulation));
            }

            if (!await unitOfWork.Set<CompanyRegulationSignature>()
                .AnyAsync(cs => cs.Signer.IsAdmin &&
                                cs.Signer.CompanyId == companyId))
            {
                throw new NotFoundException(typeof(CompanyRegulationSignature));
            }
        }

        private async Task ValidateUserRegulationAsync(Guid userId, Guid companyId)
        {
            if (!await unitOfWork.Set<UserRegulation>().AnyAsync(c => c.UserId == userId))
            {
                throw new NotFoundException(typeof(UserRegulation));
            }

            if (!await unitOfWork.Set<UserRegulationSignature>()
                .AnyAsync(us => us.Signer.IsAdmin &&
                                us.Signer.CompanyId == companyId))
            {
                throw new NotFoundException(typeof(CompanyRegulationSignature));
            }
        }

        private static void InitUser(User user, string password, int maxPasswordRetryCount)
        {
            user.Salt = SecurityToolkit.GetSalt();
            user.Password = SecurityToolkit.GetSha512Hash(password, user.Salt);
            user.SerialNumber = Guid.NewGuid().ToString("N"); // expire other logins.
            user.ActivationToken = null;
            user.ActivationTokenCreationDateTime = null;
            user.CreationDate = DateTime.UtcNow;
            user.CanSign = true;
            user.IsAdmin = true;
            user.PasswordRetryLimit = maxPasswordRetryCount;
        }

        private async Task InitCompany(User user)
        {
            if (user.Company != null)
            {
                var existingCompany = await unitOfWork
                    .Set<Company>()
                    .Include(c => c.Users)
                    .FirstOrDefaultAsync(c => c.TIN == user.Company.TIN);
                if (existingCompany != null)
                {
                    existingCompany.ShortName = user.Company.ShortName;
                    existingCompany.FullName = user.Company.FullName;
                    unitOfWork.Set<Company>().Update(existingCompany);
                    user.CompanyId = existingCompany.Id;
                    user.Company = null;
                }

                if (existingCompany == null || !existingCompany.Users.Any())
                {
                    user.IsAdmin = true;
                }
            }
        }
    }
}