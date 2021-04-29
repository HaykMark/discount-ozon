using System.Collections.Generic;
using System.Linq;
using Discounting.API.Common.ViewModels.TariffDiscounting;
using Discounting.Seeding.Constants;
using Discounting.Tests.Fixtures.Common;
using Discounting.Tests.Infrastructure;

namespace Discounting.Tests.Fixtures
{
    public class TariffFixture : BaseFixture
    {
        public TariffFixture(AppState appState) : base(appState)
        {
        }

        public List<TariffDTO> GetPayload()
        {
            return new List<TariffDTO>
            {
                new TariffDTO
                {
                    FromAmount = 1,
                    UntilAmount = 1000,
                    FromDay = 1,
                    UntilDay = 10,
                    Rate = 10,
                    UserId = GuidValues.UserGuids.TestSeller
                },
                new TariffDTO
                {
                    FromAmount = 1,
                    UntilAmount = 1000,
                    FromDay = 11,
                    UntilDay = 21,
                    Rate = 10,
                    UserId = GuidValues.UserGuids.TestSeller
                },
                new TariffDTO
                {
                    FromAmount = 1,
                    UntilAmount = 1000,
                    FromDay = 22,
                    Rate = 10,
                    UserId = GuidValues.UserGuids.TestSeller
                },
                new TariffDTO
                {
                    FromAmount = 1000.01M,
                    UntilAmount = 10000,
                    FromDay = 1,
                    UntilDay = 10,
                    Rate = 8,
                    UserId = GuidValues.UserGuids.TestSeller
                },
                new TariffDTO
                {
                    FromAmount = 1000.01M,
                    UntilAmount = 10000,
                    FromDay = 11,
                    UntilDay = 21,
                    Rate = 8,
                    UserId = GuidValues.UserGuids.TestSeller
                },
                new TariffDTO
                {
                    FromAmount = 1000.01M,
                    UntilAmount = 10000,
                    FromDay = 22,
                    Rate = 8,
                    UserId = GuidValues.UserGuids.TestSeller
                },
                new TariffDTO
                {
                    FromAmount = 10000.01M,
                    FromDay = 1,
                    UntilDay = 10,
                    Rate = 5,
                    UserId = GuidValues.UserGuids.TestSeller
                },
                new TariffDTO
                {
                    FromAmount = 10000.01M,
                    FromDay = 11,
                    UntilDay = 21,
                    Rate = 5,
                    UserId = GuidValues.UserGuids.TestSeller
                },
                new TariffDTO
                {
                    FromAmount = 10000.01M,
                    FromDay = 22,
                    Rate = 5,
                    UserId = GuidValues.UserGuids.TestSeller
                }
            };
        }

        public bool ValidateResponseWithPayload(List<TariffDTO> expected, List<TariffDTO> actual)
        {
            return !expected
                .Where((t, i) =>
                    t.FromAmount != actual[i].FromAmount ||
                    t.UntilAmount != actual[i].UntilAmount ||
                    t.FromDay != actual[i].FromDay ||
                    t.UntilDay != actual[i].UntilDay ||
                    t.Rate != actual[i].Rate)
                .Any();
        }
    }
}