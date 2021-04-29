using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discounting.Data.Context;
using Discounting.Entities.Account;
using Discounting.Entities.TariffDiscounting;
using Discounting.Logics.Account;
using Discounting.Logics.Validators;
using Microsoft.EntityFrameworkCore;

namespace Discounting.Logics.TariffDiscounting
{
    public interface ITariffService
    {
        Task<(Tariff[], int)> GetAll(int offset, int limit, Guid? companyId = null);
        Task<(TariffArchive[], int)> GetArchives(int offset, int limit, Guid? companyId = null);

        /// <summary>
        /// We always create a new set of tariffs,
        /// It should be called on Create/Update/Remove endpoints
        /// </summary>
        /// <param name="tariffs"></param>
        /// <returns></returns>
        Task<Tariff[]> CreateAsync(Tariff[] tariffs);
    }

    public class TariffService : ITariffService
    {
        private readonly IUnitOfWork unitOfWork;
        private readonly ISessionService sessionService;
        private readonly ITariffValidator tariffValidator;

        public TariffService(
            ISessionService sessionService,
            IUnitOfWork unitOfWork,
            ITariffValidator tariffValidator
        )
        {
            this.sessionService = sessionService;
            this.unitOfWork = unitOfWork;
            this.tariffValidator = tariffValidator;
        }

        public async Task<(Tariff[], int)> GetAll(int offset, int limit, Guid? companyId = null)
        {
            var query = unitOfWork.Set<Tariff>()
                .Where(t => !companyId.HasValue ||
                            t.User.CompanyId == companyId.Value);
            return (await query
                    .Skip(offset)
                    .Take(limit)
                    .ToArrayAsync(),
                await query.CountAsync());
        }

        public async Task<(TariffArchive[], int)> GetArchives(int offset, int limit, Guid? companyId = null)
        {
            var query = unitOfWork.Set<TariffArchive>()
                .Where(t => !companyId.HasValue ||
                            t.User.CompanyId == companyId.Value);
            return (await query
                    .Skip(offset)
                    .Take(limit)
                    .ToArrayAsync(),
                await query.CountAsync());
        }


        /// <summary>
        /// We always create a new set of tariffs,
        /// It should be called on Create/Update/Remove endpoints
        /// </summary>
        /// <param name="tariffs"></param>
        /// <returns></returns>
        public async Task<Tariff[]> CreateAsync(Tariff[] tariffs)
        {
            tariffValidator.Validate(tariffs);
            var userId = sessionService.GetCurrentUserId();
            InitTariffs(tariffs, userId);
            await InitArchive(userId);
            unitOfWork.AddRange(tariffs);
            await unitOfWork.SaveChangesAsync();
            return tariffs;
        }

        private static void InitTariffs(Tariff[] tariffs, Guid userId)
        {
            tariffs.First().FromDay = 1;
            tariffs.First().FromAmount = 1;
            foreach (var tariff in tariffs)
            {
                tariff.Id = Guid.NewGuid();
                tariff.UserId = userId;
                tariff.CreationDate = DateTime.UtcNow;
            }
        }

        private async Task InitArchive(Guid userId)
        {
            var currentUser = await unitOfWork.Set<User>()
                .FirstOrDefaultAsync(u => u.Id == userId);
            var tariffsToArchive = await unitOfWork.Set<Tariff>()
                .Where(t => t.User.CompanyId == currentUser.CompanyId)
                .ToListAsync();
            if (tariffsToArchive.Any())
            {
                var archives = new List<TariffArchive>();
                var groupId = Guid.NewGuid();
                tariffsToArchive.ForEach(t =>
                {
                    archives.Add(new TariffArchive
                    {
                        FromAmount = t.FromAmount,
                        UntilAmount = t.UntilAmount,
                        FromDay = t.FromDay,
                        UntilDay = t.UntilDay,
                        Rate = t.Rate,
                        CreatorId = t.UserId,
                        ActionTime = DateTime.UtcNow,
                        GroupId = groupId,
                        UserId = currentUser.Id
                    });
                });
                await unitOfWork.Set<TariffArchive>().AddRangeAsync(archives);
                unitOfWork.Set<Tariff>().RemoveRange(tariffsToArchive);
            }
        }
    }
}