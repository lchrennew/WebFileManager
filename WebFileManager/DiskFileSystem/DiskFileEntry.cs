using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.IO;

namespace WebFileManager.DiskFileSystem
{
    /// <summary>
    /// File Entries of System.IO.FileInfo
    /// </summary>
    public class DiskFileEntry : IFileEntry
    {
        public DiskFileEntry() { EndPosition = -1; }

        public FileInfo File { get; set; }

        public long StartPosition { get; set; }

        public long EndPosition { get; set; }

        Stream stream;

        public Stream OpenRead()
        {
            if (File != null)
            {
                stream = File.OpenRead();
                stream.Position = StartPosition;
            }
            return stream;
        }

        public StreamReader OpenText()
        {
            if (File != null)
            {
                return File.OpenText();
            }
            else return null;
        }

        public long Length
        {
            get
            {
                if (File != null)
                    return File.Length;
                else
                    return 0L;
            }
        }

        public string Filename
        {
            get
            {
                return File.FullName;
            }
        }


    }
}