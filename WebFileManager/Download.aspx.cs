using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.IO;
using System.Text;
using Yahoo.Yui.Compressor;
using WebFileManager.Minify;

namespace WebFileManager
{
    public partial class Download : System.Web.UI.Page
    {


        protected void Page_Load(object sender, EventArgs e)
        {
            /*
             * 需求
             * 1、支持存储扩展：磁盘文件系统..........................DONE
             * 2、支持存储扩展：MongoDB GridFS........................DONE
             * 3、支持GFS存储
             * 4、支持FTP存储
             * 5、支持下载断点续传....................................DONE
             * 6、支持文件压缩
             * 7、支持文件合并........................................DONE
             * 8、支持css/js压缩 .....................................DONE
             * 9、支持客户端缓存（强验证 弱验证）.....................DONE
             */
            Response.ClearHeaders();

            string filename = Request["f"];

            if (!string.IsNullOrEmpty(filename))
            {

                string mimeType;
                IEnumerable<string> filenames = FileHelper.ParseFilenames(filename, out mimeType);
                Response.ContentType = mimeType;

                long start = 0, end = 0, length, totalLength;

                DateTime lastModified;
                string range =
                    Request.Headers["Range"],
                    ifrangeETag = Request.Headers["If-Range"],
                    ifnmETag = Request.Headers["If-None-Match"],
                    eTag = FileHelper.GetETag(filenames, out lastModified, true);


                Response.Cache.SetMaxAge(TimeSpan.FromMinutes(1));


                // If-None-Matched的处理（304的处理）
                if (string.IsNullOrEmpty(range) && !string.IsNullOrEmpty(ifnmETag) && ifnmETag == eTag) // 在不存在Range的前提下，如果存在etag，并且匹配，则返回304（除非有range）
                {
                    End304(eTag);
                    return;
                }

                if (!string.IsNullOrEmpty(range))
                {
                    Response.AddHeader("Accept-Ranges", "bytes");
                    ParseRange(range, out start, out end);
                    //eTag = FileHelper.GetETag(lastModified, start, end);

                    // If-Range的处理
                    if (!string.IsNullOrEmpty(ifrangeETag) && ifrangeETag != eTag) // 在range存在的前提下，如果有if-range，并且客户端文件已经失效，则返回整个服务器端文件（设置起始均为0，重新设置etag）
                    {
                        start = 0;
                        end = -1;
                        //Response.StatusCode = 200;
                    }
                    else
                        Response.StatusCode = 206;  // HTTP 206: partial content(局部内容)
                }

                // 输出文件内容
                IEnumerable<IFileEntry> entries;
                if (Response.StatusCode == 206)
                {
                    entries = FileHelper.GetEntries(filenames, start, end, out totalLength, out length, out start, out end);
                    Response.AddHeader("Content-Range", string.Format(end >= totalLength - 1 ? "bytes {0}-/{2}" : "bytes {0}-{1}/{2}", start, end, totalLength));
                    if (entries == null)
                    {
                        EndResponse(416);
                        return;
                    }
                }
                else
                {
                    entries = FileHelper.GetEntries(filenames, out length);
                }
                if (!entries.Any())
                {
                    EndResponse(404);
                    return;
                }

                if (Response.StatusCode == 200)
                {
                    if (Request.QueryString["min"] != null)
                    {
                        entries = new IFileEntry[] { new MiniFileEntry { Entries = entries } };
                        length = entries.First().Length;
                    }
                    Response.Cache.SetLastModified(lastModified);
                }

                Response.AddHeader("Content-Length", length.ToString());
                Response.AddHeader("ETag", eTag);
                FileHelper.OutputTo(entries, Response.OutputStream);

                Response.End();
            }
            else
            {
                EndResponse(403);
            }
        }

        void End304(string etag)
        {
            Response.AddHeader("ETag", etag);
            EndResponse(304);
        }

        void EndResponse(int statusCode)
        {
            Response.StatusCode = statusCode;
            Response.End();
        }

        void ParseRange(string range, out long start, out long end)
        {
            string[] s = range.Split('-', '=');
            Int64.TryParse(s[1], out start);
            if (!Int64.TryParse(s[2], out end)) end = -1;
        }
    }
}