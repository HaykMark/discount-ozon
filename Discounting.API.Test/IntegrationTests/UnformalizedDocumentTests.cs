using System;
using System.Linq;
using System.Threading.Tasks;
using Discounting.API.Common.ViewModels.UnformalizedDocument;
using Discounting.Entities;
using Discounting.Seeding.Constants;
using Discounting.Tests.Fixtures;
using Discounting.Tests.Infrastructure;
using Xunit;

namespace Discounting.Tests.IntegrationTests
{
    public class UnformalizedDocumentTests : TestBase
    {
        public UnformalizedDocumentTests(AppState appState) : base(appState)
        {
            unformalizedDocumentFixture = new UnformalizedDocumentFixture(appState);
        }

        private readonly UnformalizedDocumentFixture unformalizedDocumentFixture;

        [Fact]
        public async Task Create_Draft_Created()
        {
            await unformalizedDocumentFixture.LoginSellerAsync();
            var createdDto = await unformalizedDocumentFixture.CreateTestUnformalizedDocumentDtoAsync();
            Assert.NotNull(createdDto);
            Assert.Equal(GuidValues.CompanyGuids.TestSeller, createdDto.SenderId);
            Assert.Equal(UnformalizedDocumentStatus.Draft, createdDto.Status);
            Assert.NotEmpty(createdDto.Receivers);
            Assert.Contains(createdDto.Receivers, x => x.ReceiverId == GuidValues.CompanyGuids.BankUserOne);
            Assert.Contains(createdDto.Receivers, x => x.ReceiverId == GuidValues.CompanyGuids.BankUserSecond);
        }

        [Fact]
        public async Task Create_Draft_Than_Update_Updated()
        {
            await unformalizedDocumentFixture.LoginSellerAsync();
            var createdDto = await unformalizedDocumentFixture.CreateTestUnformalizedDocumentDtoAsync();
            createdDto.Topic = "new topic";
            createdDto.Message = "new message";
            var updatedDto = await unformalizedDocumentFixture.UnformalizedDocumentApi.Put(createdDto.Id, createdDto);

            Assert.NotNull(updatedDto);
            Assert.Equal(createdDto.SenderId, updatedDto.SenderId);
            Assert.Equal(createdDto.Status, updatedDto.Status);
            Assert.Equal(createdDto.Topic, updatedDto.Topic);
            Assert.Equal(createdDto.Message, updatedDto.Message);
            Assert.Equal(createdDto.CreationDate.Date, updatedDto.CreationDate.Date);
            Assert.NotEmpty(updatedDto.Receivers);
            Assert.Contains(updatedDto.Receivers, x => x.ReceiverId == GuidValues.CompanyGuids.BankUserOne);
            Assert.Contains(updatedDto.Receivers, x => x.ReceiverId == GuidValues.CompanyGuids.BankUserSecond);
        }

        [Fact]
        public async Task Create_Draft_Than_Update_Receiver_Updated()
        {
            await unformalizedDocumentFixture.LoginSellerAsync();
            var createdDto = await unformalizedDocumentFixture.CreateTestUnformalizedDocumentDtoAsync();
            createdDto.Receivers = new[]
            {
                new UnformalizedDocumentReceiverDTO
                {
                    UnformalizedDocumentId = createdDto.Id,
                    ReceiverId = GuidValues.CompanyGuids.TestBuyer
                },
                createdDto.Receivers.First(r => r.ReceiverId == GuidValues.CompanyGuids.BankUserOne)
            };
            var updatedDto = await unformalizedDocumentFixture.UnformalizedDocumentApi.Put(createdDto.Id, createdDto);

            Assert.NotNull(updatedDto);
            Assert.Equal(createdDto.SenderId, updatedDto.SenderId);
            Assert.Equal(createdDto.Status, updatedDto.Status);
            Assert.Equal(createdDto.Topic, updatedDto.Topic);
            Assert.Equal(createdDto.CreationDate.Date, updatedDto.CreationDate.Date);
            Assert.NotEmpty(updatedDto.Receivers);
            Assert.Contains(updatedDto.Receivers, x => x.ReceiverId == GuidValues.CompanyGuids.BankUserOne);
            Assert.Contains(updatedDto.Receivers, x => x.ReceiverId == GuidValues.CompanyGuids.TestBuyer);
            Assert.DoesNotContain(updatedDto.Receivers, x => x.ReceiverId == GuidValues.CompanyGuids.BankUserSecond);
        }

        [Fact]
        public async Task Create_SignedBySender_Get_By_Receiver_Not_Null()
        {
            await unformalizedDocumentFixture.LoginSellerAsync();
            var createdDto = await unformalizedDocumentFixture.CreateTestUnformalizedDocumentDtoAsync();
            // Adjust entity
            var entity = await unformalizedDocumentFixture.DbContext.Set<UnformalizedDocument>()
                .FindAsync(createdDto.Id);
            entity.Status = UnformalizedDocumentStatus.NeedReceiverSignature;
            entity.IsSent = true;
            unformalizedDocumentFixture.DbContext.Set<UnformalizedDocument>().Update(entity);
            await unformalizedDocumentFixture.DbContext.SaveChangesAsync();

            await unformalizedDocumentFixture.LoginBankAsync();
            var documentDto = await unformalizedDocumentFixture.UnformalizedDocumentApi.Get(createdDto.Id);
            Assert.NotNull(documentDto);
            Assert.Equal(createdDto.Id, documentDto.Id);
        }

        [Fact]
        public async Task Create_Than_Get_Not_Null()
        {
            await unformalizedDocumentFixture.LoginSellerAsync();
            var expectedDto = await unformalizedDocumentFixture.CreateTestUnformalizedDocumentDtoAsync();
            var actualDto = await unformalizedDocumentFixture.UnformalizedDocumentApi.Get(expectedDto.Id);
            Assert.NotNull(actualDto);
            Assert.Equal(expectedDto.Id, actualDto.Id);
            Assert.NotEmpty(actualDto.Receivers);
            Assert.Contains(actualDto.Receivers, x => x.ReceiverId == GuidValues.CompanyGuids.BankUserOne);
            Assert.Contains(actualDto.Receivers, x => x.ReceiverId == GuidValues.CompanyGuids.BankUserSecond);
        }

        [Fact]
        public async Task Create_Than_Get_All_NotEmpty()
        {
            await unformalizedDocumentFixture.LoginSellerAsync();
            var expectedDto = await unformalizedDocumentFixture.CreateTestUnformalizedDocumentDtoAsync();
            var allDocuments = await unformalizedDocumentFixture.UnformalizedDocumentApi.GetAll();
            Assert.NotEmpty(allDocuments);
            Assert.Contains(allDocuments, x => x.Id == expectedDto.Id);
            allDocuments.ForEach(d => Assert.NotEmpty(d.Receivers));
        }

        [Fact]
        public async Task Create_NotSigned_Than_Get_All_By_Receiver_Does_Not_Contain()
        {
            await unformalizedDocumentFixture.LoginSellerAsync();
            var expectedDto = await unformalizedDocumentFixture.CreateTestUnformalizedDocumentDtoAsync();
            await unformalizedDocumentFixture.LoginBankAsync();
            var allDocuments = await unformalizedDocumentFixture.UnformalizedDocumentApi.GetAll();
            Assert.DoesNotContain(allDocuments, x => x.Id == expectedDto.Id);
        }

        [Fact]
        public async Task Create_NotSigned_Than_Decline_Declined()
        {
            await unformalizedDocumentFixture.LoginSellerAsync();
            var documentDto = await unformalizedDocumentFixture.CreateTestUnformalizedDocumentDtoAsync();
            var declinedDocumentDto = await unformalizedDocumentFixture.UnformalizedDocumentApi.Decline(
                new UnformalizedDocumentDeclineDTO
                {
                    Id = documentDto.Id,
                    DeclineReason = "test",
                });
            var declines =
                await unformalizedDocumentFixture.UnformalizedDocumentApi.GetAll(UnformalizedDocumentStatus.Declined);

            Assert.NotNull(declinedDocumentDto.DeclinedBy);
            Assert.NotNull(declinedDocumentDto.DeclinedDate);
            Assert.False(string.IsNullOrEmpty(declinedDocumentDto.DeclineReason));
            Assert.Contains(declines, d => d.Id == declinedDocumentDto.Id && 
                                           d.CreationDate.Date == documentDto.CreationDate.Date);
            declines.ForEach(d => Assert.NotEmpty(d.Receivers));
        }

        [Fact]
        public async Task Create_SignedBySender_Then_Decline_Declined()
        {
            await unformalizedDocumentFixture.LoginSellerAsync();
            var documentDto = await unformalizedDocumentFixture.CreateTestUnformalizedDocumentDtoAsync();
            // Adjust entity
            var entity = await unformalizedDocumentFixture.DbContext.Set<UnformalizedDocument>()
                .FindAsync(documentDto.Id);
            entity.Status = UnformalizedDocumentStatus.NeedReceiverSignature;
            entity.IsSent = true;
            unformalizedDocumentFixture.DbContext.Set<UnformalizedDocument>().Update(entity);
            await unformalizedDocumentFixture.DbContext.SaveChangesAsync();

            await unformalizedDocumentFixture.LoginBankAsync();
            Assert.Equal(documentDto.Id, documentDto.Id);
            var declinedDocumentDto = await unformalizedDocumentFixture.UnformalizedDocumentApi.Decline(
                new UnformalizedDocumentDeclineDTO
                {
                    Id = documentDto.Id,
                    DeclineReason = "test",
                });
            var declines =
                await unformalizedDocumentFixture.UnformalizedDocumentApi.GetAll(UnformalizedDocumentStatus.Declined);

            Assert.NotNull(declinedDocumentDto.DeclinedBy);
            Assert.NotNull(declinedDocumentDto.DeclinedDate);
            Assert.False(string.IsNullOrEmpty(declinedDocumentDto.DeclineReason));
            Assert.Contains(declines, x => x.Id == declinedDocumentDto.Id);
            declines.ForEach(d => Assert.NotEmpty(d.Receivers));
        }

        [Fact]
        public async Task Create_SignedByAll_Then_Decline_Declined()
        {
            await unformalizedDocumentFixture.LoginSellerAsync();
            var documentDto = await unformalizedDocumentFixture.CreateTestUnformalizedDocumentDtoAsync();
            // Adjust entity
            var entity = await unformalizedDocumentFixture.DbContext.Set<UnformalizedDocument>()
                .FindAsync(documentDto.Id);
            entity.Status = UnformalizedDocumentStatus.SignedByAll;
            entity.IsSent = true;
            unformalizedDocumentFixture.DbContext.Set<UnformalizedDocument>().Update(entity);
            await unformalizedDocumentFixture.DbContext.SaveChangesAsync();

            await unformalizedDocumentFixture.LoginBankAsync();
            Assert.Equal(documentDto.Id, documentDto.Id);

            await AssertHelper.AssertForbiddenAsync(() => unformalizedDocumentFixture.UnformalizedDocumentApi.Decline(
                new UnformalizedDocumentDeclineDTO
                {
                    Id = documentDto.Id,
                    DeclineReason = "test",
                }));
        }

        [Fact]
        public async Task Create_Draft_Get_Draft_NotEmpty()
        {
            await unformalizedDocumentFixture.LoginSellerAsync();
            var createdDto = await unformalizedDocumentFixture.CreateTestUnformalizedDocumentDtoAsync();
            var drafts =
                await unformalizedDocumentFixture.UnformalizedDocumentApi.GetAll(UnformalizedDocumentStatus.Draft);

            Assert.NotEmpty(drafts);
            Assert.Contains(drafts, x => x.Id == createdDto.Id);
        }

        [Fact]
        public async Task Create_Sent_Get_Sent_NotEmpty()
        {
            await unformalizedDocumentFixture.LoginSellerAsync();
            var createdDto = await unformalizedDocumentFixture.CreateTestUnformalizedDocumentDtoAsync();
            // Adjust entity
            var entity = await unformalizedDocumentFixture.DbContext.Set<UnformalizedDocument>()
                .FindAsync(createdDto.Id);
            entity.SentDate = DateTime.UtcNow;
            entity.Status = UnformalizedDocumentStatus.NeedReceiverSignature;
            entity.IsSent = true;
            unformalizedDocumentFixture.DbContext.Set<UnformalizedDocument>().Update(entity);
            await unformalizedDocumentFixture.DbContext.SaveChangesAsync();

            var sentDtos =
                await unformalizedDocumentFixture.UnformalizedDocumentApi.GetSent(
                    sentDateFrom: DateTime.UtcNow.AddHours(-1),
                    sentDateTo: DateTime.UtcNow.AddHours(1),
                    receiverId: createdDto.Receivers.First().ReceiverId
                );

            Assert.NotEmpty(sentDtos);
            Assert.Single(sentDtos);
            Assert.Contains(sentDtos, x => x.Id == createdDto.Id);
        }
        
        [Fact]
        public async Task Create_Sent_Get_Received_NotEmpty()
        {
            await unformalizedDocumentFixture.LoginSellerAsync();
            var createdDto = await unformalizedDocumentFixture.CreateTestUnformalizedDocumentDtoAsync();
            // Adjust entity
            var entity = await unformalizedDocumentFixture.DbContext.Set<UnformalizedDocument>()
                .FindAsync(createdDto.Id);
            entity.SentDate = DateTime.UtcNow;
            entity.Status = UnformalizedDocumentStatus.NeedReceiverSignature;
            entity.IsSent = true;
            unformalizedDocumentFixture.DbContext.Set<UnformalizedDocument>().Update(entity);
            await unformalizedDocumentFixture.DbContext.SaveChangesAsync();

            await unformalizedDocumentFixture.LoginBankAsync();
            var sentDtos =
                await unformalizedDocumentFixture.UnformalizedDocumentApi.GetReceived(
                    receivedDateFrom: DateTime.UtcNow.AddHours(-1),
                    receivedDateTo: DateTime.UtcNow.AddHours(1),
                    senderId: createdDto.SenderId
                );

            Assert.NotEmpty(sentDtos);
            Assert.Single(sentDtos);
            Assert.Contains(sentDtos, x => x.Id == createdDto.Id);
        }
    }
}