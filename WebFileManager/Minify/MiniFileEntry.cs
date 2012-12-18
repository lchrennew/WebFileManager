using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.IO;
using System.Text;
using Yahoo.Yui.Compressor;

namespace WebFileManager.Minify
{
    public class MiniFileEntry : IFileEntry
    {
        public MiniFileEntry() {
            EndPosition = -1;
        }

        public IEnumerable<IFileEntry> Entries { get; set; }

        public long StartPosition { get; set; }

        public long EndPosition { get; set; }

        public long Length
        {
            get
            {
                return Stream.Length;
            }
        }

        Stream stream;
        Stream Stream
        {
            get
            {
                if (stream == null)
                {
                    stream = new MemoryStream();
                    StreamWriter sw = new StreamWriter(stream);
                    sw.Write(Minified);
                }
                return stream;
            }
        }

        string min;

        string Minified
        {
            get
            {
                if (min == null)
                    if (Entries != null && Entries.Any())
                    {
                        StringBuilder sb = new StringBuilder();
                        foreach (IFileEntry entry in Entries)
                        {
                            var r = entry.OpenText();
                            sb.Append(r.ReadToEnd());
                            r.Close();
                        }
                        ICompressor compressor = null;
                        string ext = Path.GetExtension(Entries.First().Filename).ToLowerInvariant();
                        if (ext == ".css") compressor = new CssCompressor { RemoveComments = true, CompressionType = CompressionType.Standard };
                        else if (ext == ".js") compressor = new JavaScriptCompressor { };
                        else min = string.Empty;
                        min = compressor.Compress(sb.ToString()).Trim();
                    }
                    else min = string.Empty;
                return min;
            }
        }
        public Stream OpenRead()
        {
            if (Entries != null && Entries.Any())
            {
                Stream.Position = 0;
                return Stream;
            }
            else return null;
        }

        public StreamReader OpenText()
        {
            throw new NotImplementedException();
        }

        public string Filename
        {
            get { throw new NotImplementedException(); }
        }
    }
}