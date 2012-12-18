using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.IO;

namespace WebFileManager
{

    /// <summary>
    /// 文件存储
    /// </summary>
    public interface IFileStorage
    {

        /// <summary>
        /// 获取文件最后更新时间
        /// </summary>
        /// <param name="filenames"></param>
        /// <returns></returns>
        DateTime GetLastModified(IEnumerable<string> filenames);

        /// <summary>
        /// 根据文件最后更新时间获取ETag
        /// </summary>
        /// <param name="lastModified"></param>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <returns></returns>
        string GetETag(DateTime lastModified, long start = -1, long end = -1, bool weak = false);

        /// <summary>
        /// 获取完整文件流列表
        /// </summary>
        /// <param name="filenames"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        IEnumerable<IFileEntry> GetEntries(IEnumerable<string> filenames, out long length);

    }
}
