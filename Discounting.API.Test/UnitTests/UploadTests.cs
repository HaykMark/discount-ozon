using System;
using System.IO;
using System.Threading.Tasks;
using AutoMapper;
using Discounting.API.Common.Mapping;
using Discounting.API.Common.ViewModels.Signature;
using Discounting.Common.Options;
using Discounting.Data.Context;
using Discounting.Entities;
using Discounting.Entities.Templates;
using Discounting.Logics;
using Discounting.Logics.Account;
using Discounting.Seeding.Constants;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace Discounting.Tests.UnitTests
{
    public class UploadTests
    {
        private readonly IOptions<UploadOptions> option = Options.Create(new UploadOptions
        {
            Path = "Uploads",
            RegistryPath = "Registries",
            SignaturePath = "Signatures"
        });

        [Fact]
        public async Task Upload_Signature_Uploaded()
        {
            //Arrange
            var fileMock = new Mock<IFormFile>();
            //auto mapper configuration
            var mockMapper = new MapperConfiguration(cfg => { cfg.AddProfile(new MappingProfile()); });
            var mapper = mockMapper.CreateMapper();
            var unitOfWork = new Mock<IUnitOfWork>();
            var sessionService = new Mock<ISessionService>();

            //Setup mock file using a memory stream
            const string content = "Hello World from a Fake File";
            const string fileName = "test.sgn";
            const string contentType = "application/octet-stream";
            var ms = new MemoryStream();
            var writer = new StreamWriter(ms);
            await writer.WriteAsync(content);
            await writer.FlushAsync();
            ms.Position = 0;
            fileMock.Setup(_ => _.OpenReadStream()).Returns(ms);
            fileMock.Setup(_ => _.FileName).Returns(fileName);
            fileMock.Setup(_ => _.Length).Returns(ms.Length);
            fileMock.Setup(_ => _.ContentType).Returns(contentType);

            var pathProviderService = new UploadPathProviderService(option);
            var uploadService = new UploadService(
                unitOfWork.Object,
                sessionService.Object,
                pathProviderService
            );

            var file = fileMock.Object;
            //Act Create
            var result = await uploadService.UploadSignatureAsync(Guid.NewGuid(), SignatureType.Registry, file, "test.sgn");
            var dto = mapper.Map<RegistrySignatureDTO>(result);
            var path = pathProviderService.GetSignaturePath(result.Name, SignatureType.Registry);

            //Assert
            Assert.NotNull(result);
            Assert.NotNull(dto);
            Assert.NotNull(dto.Location);
            Assert.Equal(fileName, dto.Name);
            Assert.False(string.IsNullOrEmpty(dto.Location.pathname));
            Assert.True(File.Exists(path));

            //Cleanup
            File.Delete(path);
        }

        [Fact]
        public async Task Upload_Template_Uploaded()
        {
            //Arrange
            var fileMock = new Mock<IFormFile>();
            //auto mapper configuration
            var unitOfWork = new Mock<IUnitOfWork>();
            var sessionService = new Mock<ISessionService>();

            //Setup mock file using a memory stream
            const string content = "Hello World from a Fake File";
            const string fileName = "test.xlsx";
            const string contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            var ms = new MemoryStream();
            var writer = new StreamWriter(ms);
            await writer.WriteAsync(content);
            await writer.FlushAsync();
            ms.Position = 0;
            fileMock.Setup(_ => _.OpenReadStream()).Returns(ms);
            fileMock.Setup(_ => _.FileName).Returns(fileName);
            fileMock.Setup(_ => _.Length).Returns(ms.Length);
            fileMock.Setup(_ => _.ContentType).Returns(contentType);

            var pathProviderService = new UploadPathProviderService(option);
            var uploadService = new UploadService(
                unitOfWork.Object,
                sessionService.Object,
                pathProviderService
            );

            var file = fileMock.Object;
            //Act Create
            var result = await uploadService.UploadTemplateAsync(GuidValues.CompanyGuids.TestSeller, TemplateType.Registry, file);
            var path = pathProviderService.GetTemplatePath(result.Id, result.ContentType, result.Type);

            //Assert
            Assert.NotNull(result);
            Assert.True(File.Exists(path));

            //Cleanup
            File.Delete(path);
        }
    }
}