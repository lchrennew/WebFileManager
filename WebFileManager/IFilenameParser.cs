using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WebFileManager
{
    /// <summary>
    /// 文件名分析器
    /// </summary>
    public interface IFilenameParser
    {
        /// <summary>
        /// 分析文件名
        /// </summary>
        /// <param name="filename"></param>
        /// <param name="ext"></param>
        /// <returns></returns>
        IEnumerable<string> Parse(string filename, out string ext);
    }
}
