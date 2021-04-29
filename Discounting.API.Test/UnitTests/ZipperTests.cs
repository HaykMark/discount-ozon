using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Discounting.API.Common.Extensions;
using Discounting.Helpers;
using Xunit;

namespace Discounting.Tests.UnitTests
{
    public class ZipperTests
    {
        [Fact]
        public async Task Create_Zip_File_Using_Zipper_Created()
        {
            var dir = Path.Combine(
                new DirectoryInfo(Directory.GetCurrentDirectory()).Parent.Parent.Parent.FullName,
                "TestFiles");

            var files = Directory.GetFiles(dir);
            var zipItems = files
                .Select(path => new ZipItem
                {
                    Path = Path.Combine(dir, path),
                    Name = Path.GetFileName(path)
                });
            await using var zipper = new Zipper();
            var stream = zipper.Zip(zipItems);
            var destinationPath = Path.Combine(dir, "test.zip");
            WriteToFile(stream, destinationPath);
            Assert.True(File.Exists(destinationPath));
            File.Delete(destinationPath);
        }

        [Fact]
        public async Task Create_Zip_File_Twice_Using_Zipper_Created()
        {
            var dir = Path.Combine(
                new DirectoryInfo(Directory.GetCurrentDirectory()).Parent.Parent.Parent.FullName,
                "TestFiles");

            var fileNames = Directory.GetFiles(dir);
            var zipItems = fileNames
                .Select(name => new ZipItem
                {
                    Path = Path.Combine(dir, name),
                    Name = name + "ttt"
                })
                .ToArray();
            await using var zipper = new Zipper();
            var stream1 = zipper.Zip(zipItems);
            var stream2 = zipper.Zip(zipItems);
            var destinationPath1 = Path.Combine(dir, "test1.zip");
            var destinationPath2 = Path.Combine(dir, "test2.zip");
            WriteToFile(stream1, destinationPath1);
            WriteToFile(stream2, destinationPath2);
            Assert.True(File.Exists(destinationPath1));
            Assert.True(File.Exists(destinationPath2));
            File.Delete(destinationPath1);
            File.Delete(destinationPath2);
        }

        private static void WriteToFile(Stream stream,
            string destinationFile,
            int bufferSize = 4096,
            FileMode mode = FileMode.OpenOrCreate,
            FileAccess access = FileAccess.ReadWrite,
            FileShare share = FileShare.ReadWrite)
        {
            using var destinationFileStream = new FileStream(destinationFile, mode, access, share);
            while (stream.Position < stream.Length)
            {
                destinationFileStream.WriteByte((byte) stream.ReadByte());
            }
        }
    }
}