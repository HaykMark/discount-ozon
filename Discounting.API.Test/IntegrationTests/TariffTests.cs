using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Discounting.API.Common.ViewModels.TariffDiscounting;
using Discounting.Tests.Fixtures;
using Discounting.Tests.Infrastructure;
using Xunit;

namespace Discounting.Tests.IntegrationTests
{
    public class TariffTests : TestBase
    {
        public TariffTests(AppState appState) : base(appState)
        {
            tariffFixture = new TariffFixture(appState);
        }

        private readonly TariffFixture tariffFixture;

        [Fact]
        public async Task Creat_UntilAmountIsNotContinuation_UnprocessableEntity()
        {
            await tariffFixture.LoginSellerAsync();
            var payload = tariffFixture.GetPayload();
            payload.First().UntilAmount -= 1;
            await AssertHelper.FailOnStatusCodeAsync(() => tariffFixture.TariffApi.Post(payload),
                HttpStatusCode.UnprocessableEntity);
        }

        [Fact]
        public async Task Creat_UntilAmountIsNull_UnprocessableEntity()
        {
            await tariffFixture.LoginSellerAsync();
            var payload = tariffFixture.GetPayload();
            payload.First().UntilAmount = null;
            await AssertHelper.FailOnStatusCodeAsync(() => tariffFixture.TariffApi.Post(payload),
                HttpStatusCode.UnprocessableEntity);
        }

        [Fact]
        public async Task Create_AsAdmin_RightData_GetAll_NotEmpty()
        {
            await tariffFixture.LoginAdminAsync();
            var payload = tariffFixture.GetPayload();
            await tariffFixture.TariffApi.Post(payload);

            var tariffs = await tariffFixture.TariffApi.GetAll();
            Assert.NotEmpty(tariffs);
        }

        [Fact]
        public async Task Create_AsSeller_RightData_GetAll_WithoutCompanyIdFilter_NotEmpty()
        {
            await tariffFixture.LoginSellerAsync();
            var payload = tariffFixture.GetPayload();
            await tariffFixture.TariffApi.Post(payload);

            await AssertHelper.AssertForbiddenAsync(() => tariffFixture.TariffApi.GetAll());
        }

        //TODO fix this test
        // [Fact]
        // public async Task Create_ThenUpdate_OldData_Archived()
        // {
        //     var sellerSessionDto = await tariffFixture.LoginSellerAsync();
        //     var payload = tariffFixture.GetPayload();
        //     var tariffs = await tariffFixture.TariffApi.Post(payload);
        //     tariffs.Last().UntilAmount = payload.Last().FromAmount + 100;
        //     tariffs.Last().UntilDay = payload.Last().FromDay + 100;
        //
        //     var newTariff = new TariffDTO
        //     {
        //         FromAmount = tariffs.Last().UntilAmount.Value + 0.01M,
        //         FromDay = tariffs.Last().UntilDay.Value + 1,
        //         Rate = 1
        //     };
        //     tariffs.Add(newTariff);
        //     await tariffFixture.TariffApi.Post(tariffs);
        //     var updatedTariffs = await tariffFixture.CompanyApi.GetTariffs(sellerSessionDto.User.CompanyId);
        //     Assert.True(tariffFixture.ValidateResponseWithPayload(tariffs, updatedTariffs));
        //     await tariffFixture.LoginAdminAsync();
        //     var archived = await tariffFixture.TariffApi.GetAllArchives(companyId: sellerSessionDto.User.CompanyId);
        //     Assert.Equal(payload.Count, archived.Count);
        //     Assert.True(archived.Select(a => a.GroupId).Distinct().Count() == 1);
        // }

        [Fact]
        public async Task Create_LastUntilAmountNotNull_UnprocessableEntity()
        {
            await tariffFixture.LoginSellerAsync();
            var payload = tariffFixture.GetPayload();
            payload.Last().UntilAmount = 100000;
            await AssertHelper.FailOnStatusCodeAsync(() => tariffFixture.TariffApi.Post(payload),
                HttpStatusCode.UnprocessableEntity);
        }

        [Fact]
        public async Task Create_LastUntilDayNotNull_UnprocessableEntity()
        {
            await tariffFixture.LoginSellerAsync();
            var payload = tariffFixture.GetPayload();
            payload.Last().UntilDay = 100000;
            await AssertHelper.FailOnStatusCodeAsync(() => tariffFixture.TariffApi.Post(payload),
                HttpStatusCode.UnprocessableEntity);
        }

        [Fact]
        public async Task Create_OneData_Created()
        {
            await tariffFixture.LoginSellerAsync();
            var payload = new List<TariffDTO>(new[]
            {
                new TariffDTO
                {
                    FromDay = 1,
                    UntilDay = null,
                    FromAmount = 1,
                    UntilAmount = null
                }
            });
            var tariffs = await tariffFixture.TariffApi.Post(payload);

            Assert.NotEmpty(tariffs);
            Assert.Single(tariffs);
            Assert.True(tariffFixture.ValidateResponseWithPayload(payload, tariffs));
        }

        [Fact]
        public async Task Create_RightData_Created()
        {
            await tariffFixture.LoginSellerAsync();
            var payload = tariffFixture.GetPayload();
            var tariffs = await tariffFixture.TariffApi.Post(payload);
            Assert.NotEmpty(tariffs);
            Assert.True(tariffFixture.ValidateResponseWithPayload(payload, tariffs));
        }

        [Fact]
        public async Task Create_RightData_Get_NotEmpty()
        {
            var sessionDto = await tariffFixture.LoginSellerAsync();
            var payload = tariffFixture.GetPayload();
            await tariffFixture.TariffApi.Post(payload);

            var tariffs = await tariffFixture.CompanyApi.GetTariffs(sessionDto.User.CompanyId);
            Assert.NotEmpty(tariffs);
        }

        [Fact]
        public async Task Create_RightData_GetAll_WithCompanyIdFilter_NotEmpty()
        {
            var sessionDto = await tariffFixture.LoginSellerAsync();
            var payload = tariffFixture.GetPayload();
            await tariffFixture.TariffApi.Post(payload);

            var tariffs = await tariffFixture.TariffApi.GetAll(companyId: sessionDto.User.CompanyId);
            Assert.NotEmpty(tariffs);
            Assert.True(tariffFixture.ValidateResponseWithPayload(payload, tariffs));
        }

        [Fact]
        public async Task Create_UntilDayIsNotContinuation_UnprocessableEntity()
        {
            await tariffFixture.LoginSellerAsync();
            var payload = tariffFixture.GetPayload();
            payload.First().UntilDay -= 1;
            await AssertHelper.FailOnStatusCodeAsync(() => tariffFixture.TariffApi.Post(payload),
                HttpStatusCode.UnprocessableEntity);
        }

        [Fact]
        public async Task Create_UntilDayIsNull_UnprocessableEntity()
        {
            await tariffFixture.LoginSellerAsync();
            var payload = tariffFixture.GetPayload();
            payload.First().UntilDay = null;
            await AssertHelper.FailOnStatusCodeAsync(() => tariffFixture.TariffApi.Post(payload),
                HttpStatusCode.UnprocessableEntity);
        }

        [Fact]
        public async Task NoLogin_IsUnauthorised()
        {
            await AssertHelper.AssertUnauthorizedAsync(() => tariffFixture.TariffApi.Get(new Guid()));
            await AssertHelper.AssertUnauthorizedAsync(() => tariffFixture.TariffApi.GetAll());
            await AssertHelper.AssertUnauthorizedAsync(() => tariffFixture.TariffApi.Post(null));
        }
    }
}