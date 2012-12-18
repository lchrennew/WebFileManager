using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.IO;
using System.Xml.Serialization;
using System.Web.Script.Serialization;

namespace WebFileManager
{
    public static class MimeType
    {
        static Dictionary<string, string> mimemap = new Dictionary<string, string>();
        static MimeType()
        {
            using (Stream stream = typeof(MimeType).Assembly.GetManifestResourceStream("WebFileManager.mimemap.json"))
            {
                mimemap = new JavaScriptSerializer().Deserialize<Dictionary<string, string>>(new StreamReader(stream).ReadToEnd());
            }
        }

        /// <summary>
        /// 获取MIME-Type
        /// </summary>
        /// <param name="extension"></param>
        /// <returns></returns>
        public static string Map(string extension)
        {
            return mimemap.ContainsKey(extension.ToLowerInvariant()) ? mimemap[extension] : null;
        }
    }
}