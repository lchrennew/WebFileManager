using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.IO;
using System.Configuration;
using System.Text;

namespace WebFileManager
{
    public static class FileHelper
    {
        static IFileStorage storage = Activator.CreateInstance(Type.GetType(ConfigurationManager.AppSettings["IFileStorage"], false, true)) as IFileStorage;
        static IFilenameParser fnParser = Activator.CreateInstance(Type.GetType(ConfigurationManager.AppSettings["IFilenameParser"], false, true)) as IFilenameParser;

        /// <summary>
        /// 解析文件名列表
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        public static IEnumerable<string> ParseFilenames(string filename, out string mimeType)
        {
            var result = fnParser.Parse(filename, out mimeType);
            mimeType = MimeType.Map(mimeType.ToLowerInvariant());
            return result;
        }

        /// <summary>
        /// 获取最后更改日期
        /// </summary>
        /// <param name="filenames"></param>
        /// <returns></returns>
        static DateTime GetLastModified(IEnumerable<string> filenames)
        {
            return storage.GetLastModified(filenames);
        }

        /// <summary>
        /// 获取ETag
        /// </summary>
        /// <param name="filenames"></param>
        /// <param name="lastModified"></param>
        /// <param name="weak">是否为弱验证器</param>
        /// <returns></returns>
        public static string GetETag(IEnumerable<string> filenames, out DateTime lastModified, bool weak = false)
        {
            lastModified = GetLastModified(filenames);
            return GetETag(lastModified, weak: weak);
        }

        /// <summary>
        /// 获取ETag
        /// </summary>
        /// <param name="lastModified"></param>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <returns></returns>
        public static string GetETag(DateTime lastModified, long start = -1, long end = -1, bool weak = false)
        {
            return storage.GetETag(lastModified, start, end, weak);
        }


        /// <summary>
        /// 获取所有的流
        /// </summary>
        /// <param name="filenames"></param>
        /// <returns></returns>
        public static IEnumerable<IFileEntry> GetEntries(IEnumerable<string> filenames, out long length)
        {
            return storage.GetEntries(filenames, out length);
        }

        /// <summary>
        /// 获取局部文件流列表
        /// </summary>
        /// <param name="filenames"></param>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <param name="totalLength"></param>
        /// <param name="actualLength"></param>
        /// <param name="actualStart"></param>
        /// <param name="actualEnd"></param>
        /// <returns></returns>
        public static IEnumerable<IFileEntry> GetEntries(IEnumerable<string> filenames, long start, long end, out long totalLength, out long actualLength, out long actualStart, out long actualEnd)
        {
            if (filenames != null && filenames.Any())
            {
                actualStart = start;
                actualEnd = end;
                IEnumerable<IFileEntry> entries = GetEntries(filenames, out totalLength);
                if (entries.Any())
                {
                    if (start >= totalLength - 1) actualStart = 0;
                    if (end < 0) actualEnd = totalLength - 1;
                    actualLength = actualEnd - actualStart + 1;
                    if (end >= totalLength || start >= totalLength || (end >= 0 && start > end)) return null;
                    long slot = 0, slot0 = 0;
                    int skip = 0;
                    int take = 0;
                    foreach (IFileEntry file in entries)
                    {
                        slot += file.Length;
                        if (start >= slot)  // skip掉它！
                        {
                            skip++;
                            slot0 = slot;
                        }
                        else if (end < 0) // 全部读取，则直接中断循环即可
                        {
                            take = entries.Count() - skip;
                            break;
                        }
                        else if (end <= slot)    // take它！为什么用<=而不是<？ 答：如果end=slot，则表示取序号0的1个字节，所以这个entry还是应该take
                        {
                            take++;
                        }
                        else break;
                    }
                    entries = entries.Skip(skip).Take(take).ToArray();
                    IFileEntry firstFile = entries.First();
                    firstFile.StartPosition = start - slot0;
                    if (end >= 0)
                    {
                        IFileEntry lastFile = entries.Last();
                        lastFile.EndPosition = lastFile.Length + end - slot;
                    }
                    return entries;
                }
            }
            totalLength = actualLength = actualStart = 0;
            actualEnd = -1;
            return new IFileEntry[0];
        }

        /// <summary>
        /// 将所有流输出到另一个流
        /// </summary>
        /// <param name="entries"></param>
        /// <param name="outputStream"></param>
        public static void OutputTo(IEnumerable<IFileEntry> entries, Stream outputStream)
        {
            foreach (IFileEntry entry in entries)
            {
                Stream stream = entry.OpenRead();
                stream.CopyTo(outputStream, entry.StartPosition, entry.EndPosition);
                stream.Close();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="destination"></param>
        /// <param name="startPosition"></param>
        /// <param name="endPosition">读取最后一字节的位置，如果此位置小于0，则标识直到读完</param>
        public static void CopyTo(this Stream stream, Stream destination, long startPosition, long endPosition)
        {
            int bufferSize = 1048576;
            byte[] buffer = new byte[bufferSize];
            stream.Position = startPosition;

            if (endPosition < 0)
            {
                stream.CopyTo(destination, bufferSize);
            }
            else
            {
                int firstReadLength = (int)((endPosition - startPosition + 1) % bufferSize);
                if (endPosition < 0) endPosition = stream.Length - 1;
                stream.Read(buffer, 0, firstReadLength);
                destination.Write(buffer, 0, firstReadLength);

                while (stream.Position <= endPosition)  // 为什么要<=而不是<呢？答：如果bufferSize=1时，最后一次读就出现等于的情况
                {
                    stream.Read(buffer, 0, bufferSize);
                    destination.Write(buffer, 0, bufferSize);
                }
            }

        }
    }
}