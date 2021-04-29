using System.Threading.Tasks;
using Discounting.API.Common.ViewModels.UnformalizedDocument;
using Discounting.Entities;
using Discounting.Seeding.Constants;
using Discounting.Tests.Fixtures.Common;
using Discounting.Tests.Infrastructure;

namespace Discounting.Tests.Fixtures
{
    public class UnformalizedDocumentFixture : BaseFixture
    {
        public UnformalizedDocumentFixture(AppState appState) : base(appState)
        {
        }

        public Task<UnformalizedDocumentDTO> CreateTestUnformalizedDocumentDtoAsync(UnformalizedDocumentDTO payload = null)
        {
            payload ??= GetPayload();
            return UnformalizedDocumentApi.Post(payload);
        }

        public UnformalizedDocumentDTO GetPayload()
        {
            return new UnformalizedDocumentDTO
            {
                Message = "test message",
                Topic = "test topic",
                Type = UnformalizedDocumentType.UnformalizedDocument,
                Receivers = new UnformalizedDocumentReceiverDTO[2]
                {
                    new UnformalizedDocumentReceiverDTO
                    {
                        ReceiverId = GuidValues.CompanyGuids.BankUserOne
                    },
                    new UnformalizedDocumentReceiverDTO
                    {
                        ReceiverId = GuidValues.CompanyGuids.BankUserSecond
                    }
                }
            };
        }
    }
}