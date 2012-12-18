using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.IO;

namespace WebFileManager.DiskFileSystem
{

    /// <summary>
    /// 磁盘文件存储
    /// </summary>
    public class DiskStorage : IFileStorage
    {

        public DateTime GetLastModified(IEnumerable<string> filenames)
        {
            if (filenames != null && filenames.Any())
                return filenames.Select(x => File.GetLastWriteTimeUtc(x)).OrderByDescending(x => x).First();
            else return DateTime.MinValue;
        }

        public string GetETag(DateTime lastModified, long start = -1, long end = -1, bool weak=false)
        {
            if (start >= 0)
                if (end >= 0) return string.Format("{4}{0:x}:{1:x}:{2:x}", lastModified.Ticks, start, end, weak?"W/":null);
                else return string.Format("{2}{0:x}:{1:x}", lastModified.Ticks, start, weak ? "W/" : null);
            else return string.Format("{1}{0:x}", lastModified.Ticks, weak ? "W/" : null);
        }

        public IEnumerable<IFileEntry> GetEntries(IEnumerable<string> filenames, out long length)
        {
            if (filenames != null && filenames.Any())
            {
                IEnumerable<IFileEntry> entries =
                    filenames
                    .Select(x => new DiskFileEntry { File = new FileInfo(x) })
                    .ToArray();
                length = GetLength(entries);
                return entries;
            }
            else { length = 0; return new IFileEntry[0]; }
        }

        /// <summary>
        /// 获取总长度
        /// </summary>
        /// <param name="entries"></param>
        /// <returns></returns>
        static long GetLength(IEnumerable<IFileEntry> entries)
        {
            if (entries != null && entries.Any())
                return entries.Sum(x => x.Length);
            else return 0;
        }
    }
}