using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.IO;

namespace WebFileManager.GridFS
{
    /// <summary>
    /// GridFS文件名分析器
    /// </summary>
    public class GridFilenameParser : IFilenameParser
    {
        public IEnumerable<string> Parse(string filename, out string ext)
        {
            string[] flist = filename.Split('$').ToArray();
            if (flist.Length > 0)
            {
                string ext1 = ext = Path.GetExtension(flist.Last());
                flist[flist.Length - 1] = Path.GetFileNameWithoutExtension(flist[flist.Length - 1]);
                return flist.Distinct().Select(x => string.Join("", x, ext1));
            }
            else
            {
                ext = null;
                return new string[0];
            }
        }
    }
}