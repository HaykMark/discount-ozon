using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;

namespace Discounting.Extensions
{
    /// <summary>
    /// Extensions fot system IO types.
    /// </summary>
    public static class SystemIOExtensions
    {
        public static string ReadEmbeddedResource<T>(this string fileName)
        {
            var assembly = typeof(T).GetTypeInfo().Assembly;
            var name = assembly.GetManifestResourceNames().FirstOrDefault(r => r.EndsWith("." + fileName, StringComparison.InvariantCultureIgnoreCase));
            if(name == null)
            {
                return null;
            }

            using var stream = assembly.GetManifestResourceStream(name);
            using var reader = new StreamReader(stream ?? throw new ArgumentNullException());
            return reader.ReadToEnd();
        }

        /// <summary>
        /// Create a zip file stream from given Directory
        /// </summary>
        /// <param name="directorySelected"></param>
        /// <returns></returns>
        public static byte[] Compress(this DirectoryInfo directorySelected)
        {
            using var zipStream = new MemoryStream();
            using var zip = new ZipArchive(zipStream, ZipArchiveMode.Create, true);
            foreach (var fileToCompress in directorySelected.GetFiles())
            {
                using var originalFileStream = fileToCompress.OpenRead();
                var entry = zip.CreateEntry(Path.GetFileName(originalFileStream.Name));
                using var entryStream = entry.Open();
                originalFileStream.CopyTo(entryStream);
            }

            return zipStream.ToArray();
        }
    }
}