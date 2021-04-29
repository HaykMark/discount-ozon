using System;
using System.IO;
using System.Threading.Tasks;
using Discounting.API.Common.ViewModels.Signature;
using Discounting.Entities;
using Discounting.Tests.Fixtures.Common;
using Discounting.Tests.Infrastructure;
using Xunit;

namespace Discounting.Tests.IntegrationTests
{
    public class SignatureTests: TestBase
    {
        public SignatureTests(AppState appState) : base(appState)
        {
            baseFixture = new BaseFixture(appState);
        }

        private readonly BaseFixture baseFixture;

        [Fact]
        public async Task VerifySignature_GoodData_Success()
        {
            await baseFixture.LoginAdminAsync();
            var dir = Path.Combine(
                new DirectoryInfo(Directory.GetCurrentDirectory()).Parent.Parent.Parent.FullName,
                "TestFiles");
            var signaturePath = Path.Combine(dir, "DP_IZVPOL_2BM-9721061415-772101001-201806060153125415820_2BM-7414003633-201211300600368180_SGN_1.sgn");
            var originalPath = Path.Combine(dir, "DP_IZVPOL_2BM-9721061415-772101001-201806060153125415820_2BM-7414003633-201211300600368180.xml");
            await using var fs = File.OpenRead(originalPath);

            var signatureBytes = await File.ReadAllBytesAsync(signaturePath);
            var originalBytes = await File.ReadAllBytesAsync(originalPath);
            var data = new SignatureVerificationRequestDTO
            {
                Type = SignatureType.Registry,
                Original = Convert.ToBase64String(originalBytes),
                Signature = Convert.ToBase64String(signatureBytes)
            };
            var result = await baseFixture.SignatureApi.VerifySignature(data);
            Assert.NotNull(result);
        }
    }
}