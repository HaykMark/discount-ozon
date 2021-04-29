using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;

namespace Discounting.Helpers
{
    public class Zipper : IAsyncDisposable
    {
        private readonly List<FileStream> fileStreams = new List<FileStream>();

        public Stream Zip(IEnumerable<ZipItem> zipItems)
        {
            var zipStream = new MemoryStream();

            using (var zip = new ZipArchive(zipStream, ZipArchiveMode.Create, true))
            {
                foreach (var zipItem in zipItems)
                {
                    var entry = zip.CreateEntry(zipItem.Name);
                    var fileStream = File.OpenRead(zipItem.Path);
                    fileStreams.Add(fileStream);
                    using var entryStream = entry.Open();
                    fileStream.CopyTo(entryStream);
                }
            }

            zipStream.Position = 0;
            return zipStream;
        }

        public async ValueTask DisposeAsync()
        {
            if (fileStreams.Any())
            {
                foreach (var fileStream in fileStreams)
                {
                    await fileStream.DisposeAsync();
                }
            }
        }
    }

    public class ZipItem
    {
        public string Path { get; set; }
        public string Name { get; set; }
    }
}