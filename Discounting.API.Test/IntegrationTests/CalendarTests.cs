using System.Linq;
using System.Threading.Tasks;
using Discounting.Tests.Fixtures;
using Discounting.Tests.Fixtures.CompanyAggregates;
using Discounting.Tests.Infrastructure;
using Xunit;

namespace Discounting.Tests.IntegrationTests
{
    public class CalendarTests : TestBase
    {
        public CalendarTests(AppState appState) : base(appState)
        {
            calendarFixture = new CalendarFixture(appState);
        }

        private readonly CalendarFixture calendarFixture;

        [Fact]
        public async Task Create_Created()
        {
            await calendarFixture.LoginAdminAsync();
            var freeDays = await calendarFixture.CreateFreeDaysAsync();
            Assert.NotNull(freeDays);
            Assert.NotEmpty(freeDays);
            Assert.True(freeDays.All(f => f.Id != default));
        }

        [Fact]
        public async Task Create_Then_Get_Not_Empty()
        {
            await calendarFixture.LoginAdminAsync();
            var expected = await calendarFixture.CreateFreeDaysAsync();
            var actual = await calendarFixture.CalendarApi.Get();
            Assert.NotEmpty(actual);
            foreach (var freeDayDto in expected)
            {
                Assert.Contains(actual, f => f.Id == freeDayDto.Id);
            }
        }

        [Fact]
        public async Task Create_Then_Deactivate_All_Then_Get_Empty()
        {
            await calendarFixture.LoginAdminAsync();
            var freeDayDtos = await calendarFixture.CreateFreeDaysAsync();
            freeDayDtos.ForEach(f => f.IsActive = false);
            var updatedFreeDays = await calendarFixture.CreateFreeDaysAsync(freeDayDtos);
            Assert.True(updatedFreeDays.All(f => f.IsActive == false));
            var freeDaysGetResult = await calendarFixture.CalendarApi.Get();
            Assert.Empty(freeDaysGetResult);
        }

        [Fact]
        public async Task Create_Then_Deactivate_All_Then_Get_WithDeactivated_NotEmpty()
        {
            await calendarFixture.LoginAdminAsync();
            var freeDayDtos = await calendarFixture.CreateFreeDaysAsync();
            freeDayDtos.ForEach(f => f.IsActive = false);
            var updatedFreeDays = await calendarFixture.CreateFreeDaysAsync(freeDayDtos);
            Assert.True(updatedFreeDays.All(f => f.IsActive == false));
            var freeDaysGetResult = await calendarFixture.CalendarApi.Get(withDeactivated: true);
            Assert.NotEmpty(freeDaysGetResult);
        }

        [Fact]
        public async Task Create_Then_Deactivate_Last_Then_Get_NotEmpty()
        {
            await calendarFixture.LoginAdminAsync();
            var freeDayDtos = await calendarFixture.CreateFreeDaysAsync();
            freeDayDtos.Last().IsActive = false;
            var updatedFreeDays = await calendarFixture.CreateFreeDaysAsync(freeDayDtos);
            var freeDaysGetResult = await calendarFixture.CalendarApi.Get();
            Assert.NotEmpty(freeDaysGetResult);
        }
    }
}