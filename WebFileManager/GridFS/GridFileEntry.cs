using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MongoDB.Driver.GridFS;
using System.IO;

namespace WebFileManager.GridFS
{
    /// <summary>
    /// GridFS文件条目
    /// </summary>
    public class GridFileEntry : IFileEntry
    {
        public GridFileEntry()
        {
            EndPosition = -1;
        }

        public MongoGridFSFileInfo File { get; set; }

        Stream stream;

        public Stream OpenRead()
        {
            if (File != null)
                stream = File.OpenRead();
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

        public long StartPosition { get; set; }

        public long EndPosition { get; set; }

        public long Length
        {
            get
            {
                if (File != null) return File.Length;
                else return 0L;
            }
        }

        public string Filename
        {
            get { return File.Name; }
        }
    }
}
