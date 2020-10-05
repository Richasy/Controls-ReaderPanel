using System;
using System.IO;
using System.IO.Compression;
using Richasy.Controls.Reader.Models.Epub.Format;

namespace Richasy.Controls.Reader.Models.Epub
{
    public class EpubArchive
    {
        private readonly ZipArchive archive;


        public  EpubArchive(byte[] epubData)
        {
            archive = Open(new MemoryStream(epubData), false);
        }

        public EpubArchive(Stream stream, bool leaveOpen)
        {
            archive = Open(stream, leaveOpen);
        }

        private ZipArchive Open(Stream stream, bool leaveOpen)
        {
            return new ZipArchive(stream, ZipArchiveMode.Read, leaveOpen, Constants.DefaultEncoding);
        }

        /// <summary>
        /// Returns an archive entry or null if not found.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public ZipArchiveEntry FindEntry(string path)
        {
            if (string.IsNullOrWhiteSpace(path)) throw new ArgumentNullException(nameof(path));
            return archive.TryGetEntryImproved(path);
        }
    }
}
