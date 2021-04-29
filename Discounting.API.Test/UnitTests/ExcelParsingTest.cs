using System.IO;
using System.Threading.Tasks;
using Discounting.Logics.Excel;
using Discounting.Tests.Infrastructure;
using Xunit;

namespace Discounting.Tests.UnitTests
{
    public class ExcelParsingTest : TestBase
    {
        [Fact]
        public async Task Upload_RightAndWrongSupplyData_DataReturned_ErrorsReturned()
        {
            var structureService = new ExcelDocumentReaderService();

            var dir = Path.Combine(
                new DirectoryInfo(Directory.GetCurrentDirectory()).Parent.Parent.Parent.FullName,
                "TestFiles");
            var path = Path.Combine(dir, "MixedSupplyExcel.xlsx");
            await using var fs = File.OpenRead(path);
            var (supplies, errors, failedCount) = structureService.ParseExcelToSupplies(fs);
            Assert.NotEmpty(supplies);
            Assert.NotEmpty(errors);
            Assert.True(failedCount > 0);
        }

        [Fact]
        public async Task Upload_RightSupplyData_NoErrors_DataReturned()
        {
            var structureService = new ExcelDocumentReaderService();

            var dir = Path.Combine(
                new DirectoryInfo(Directory.GetCurrentDirectory()).Parent.Parent.Parent.FullName,
                "TestFiles");
            var path = Path.Combine(dir, "RightSupplyExcel.xlsx");
            await using var fs = File.OpenRead(path);
            var (supplies, errors, failedCount) = structureService.ParseExcelToSupplies(fs);
            Assert.Empty(errors);
            Assert.NotEmpty(supplies);
            Assert.Equal(5, supplies.Count);
            Assert.Equal(0, failedCount);
        }

        [Fact]
        public async Task Upload_Template_NoErrors_DataReturned()
        {
            var structureService = new ExcelDocumentReaderService();

            var dir = Path.Combine(
                new DirectoryInfo(Directory.GetCurrentDirectory()).Parent.Parent.Parent.FullName,
                "TestFiles");
            var path = Path.Combine(dir, "Template.xlsx");
            await using var fs = File.OpenRead(path);
            var (supplies, errors, failedCount) = structureService.ParseExcelToSupplies(fs);
            Assert.Empty(errors);
            Assert.NotEmpty(supplies);
            Assert.Single(supplies);
            Assert.Equal(0, failedCount);
        }

        [Fact]
        public async Task Upload_WrongSupplyData_NoDateReturned_ErrorsReturned()
        {
            var structureService = new ExcelDocumentReaderService();

            var dir = Path.Combine(
                new DirectoryInfo(Directory.GetCurrentDirectory()).Parent.Parent.Parent.FullName,
                "TestFiles");
            var path = Path.Combine(dir, "WrongSupplyExcel.xlsx");
            await using var fs = File.OpenRead(path);
            var (supplies, errors, failedCount) = structureService.ParseExcelToSupplies(fs);
            Assert.Empty(supplies);
            Assert.Equal(12, failedCount);
            Assert.NotEmpty(errors);
        }
    }
}