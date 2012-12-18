using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace WebFileManager
{
    /// <summary>
    /// 文件条目
    /// </summary>
    public interface IFileEntry
    {
        /// <summary>
        /// 当前条目的文件名
        /// </summary>
        string Filename { get; }

        /// <summary>
        /// 当前文件条目读取起点
        /// </summary>
        long StartPosition { get; set; }

        /// <summary>
        /// 当前文件条目读取结束点
        /// </summary>
        long EndPosition { get; set; }

        /// <summary>
        /// 文件长度
        /// </summary>
        long Length { get; }

        /// <summary>
        /// 打开读取流
        /// </summary>
        /// <returns></returns>
        Stream OpenRead();

        /// <summary>
        /// 打开文本
        /// </summary>
        /// <returns></returns>
        StreamReader OpenText();

    }
}
