using System;
using System.Threading.Tasks;
using Discounting.API.Common.ViewModels.Regulations;
using Discounting.Entities.Regulations;
using Discounting.Tests.Fixtures.Common;
using Discounting.Tests.Infrastructure;

namespace Discounting.Tests.Fixtures
{
    public class UserRegulationFixture : BaseFixture
    {
        public UserRegulationFixture(AppState appState) : base(appState)
        {
        }

        public async Task<UserRegulationDTO> CreateTestUserRegulationAsync(UserRegulationDTO payload = null)
        {
            if (!(payload is null))
                return await UserRegulationApi.Post(payload);
            var user = await UserFixture.CreateTestUserAsync();
            payload = GetPayload(user.Id);
            return await UserRegulationApi.Post(payload);
        }

        public UserRegulationDTO GetPayload(Guid userId, UserRegulationType type = UserRegulationType.Profile)
        {
            return new UserRegulationDTO
            {
                Type = type,
                UserId = userId,
                UserProfileRegulationInfo = new UserProfileRegulationInfoDTO
                {
                    Citizenship = "cit test",
                    Date = DateTime.Now,
                    Number = "number test",
                    IdentityDocument = "id test",
                    IsResident = true,
                    PassportDate = DateTime.Now,
                    PassportNumber = "pass test",
                    PassportSeries = "pass ser test",
                    DateOfBirth = DateTime.Now,
                    PassportUnitCode = "pass unit code test",
                    PassportSNILS = "snils test",
                    PassportIssuingAuthorityPSRN = "psnr test",
                    PlaceOfBirth = "place test",
                    AuthorityValidityDate = DateTime.Now
                }
            };
        }
    }
}