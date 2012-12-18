using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MongoDB.Driver.GridFS;
using MongoDB.Driver;
using System.Configuration;
using MongoDB.Driver.Builders;
using MongoDB.Bson;
using System.IO;

namespace WebFileManager.GridFS
{
    /// <summary>
    /// GridFS文件存储
    /// </summary>
    public class GridStorage : IFileStorage
    {
        static MongoGridFS gfs = new MongoGridFS(MongoDatabase.Create(ConfigurationManager.ConnectionStrings["gfs"].ConnectionString));

        public DateTime GetLastModified(IEnumerable<string> filenames)
        {
            return gfs
                .Find(Query.In("filename", new BsonArray(filenames)))
                .SetSortOrder(SortBy.Descending("uploadDate"))
                .SetLimit(1)
                .Select(x => x.UploadDate).FirstOrDefault();
        }

        public string GetETag(DateTime lastModified, long start = -1, long end = -1, bool weak = false)
        {
            if (start >= 0)
                if (end >= 0) return string.Format("{4}{0:x}:{1:x}:{2:x}", lastModified.Ticks, start, end, weak ? "W/" : null);
                else return string.Format("{2}{0:x}:{1:x}", lastModified.Ticks, start, weak ? "W/" : null);
            else return string.Format("{1}{0:x}", lastModified.Ticks, weak ? "W/" : null);
        }

        public IEnumerable<IFileEntry> GetEntries(IEnumerable<string> filenames, out long length)
        {
            if (filenames != null && filenames.Any())
            {
                MongoGridFSFileInfo[] files = gfs.Find(Query.In("filename", new BsonArray(filenames))).ToArray();
                length = files.Sum(x => x.Length);
                return files.Select(x => new GridFileEntry { File = x });
            }
            else { length = 0; return new IFileEntry[0]; }
        }
    }
}