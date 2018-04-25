using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Project.Ashx
{
    /// <summary>
    /// download生成的Excel文件
    /// </summary>
    public class download : IHttpHandler
    {

        public void ProcessRequest(HttpContext context)
        {
            try
            {
                string downUrl = context.Request.QueryString["downurl"];//路径
                context.Response.Buffer = true;
                context.Response.Clear();
                context.Response.ContentType = "application/octet-stream";
                string downFile = ""; //记录下载文件的名称 

                var strtt = downUrl;
                downFile = strtt.Substring(strtt.LastIndexOf('/') + 1);
                string EncodeFileName = HttpUtility.UrlEncode(downFile, System.Text.Encoding.UTF8);//防止中文出现乱码
                context.Response.AddHeader("Content-Disposition", "attachment;filename=" + EncodeFileName + ";");
                string name = context.Server.MapPath(downUrl);
                byte[] bytes = System.IO.File.ReadAllBytes(name);
                context.Response.BinaryWrite(bytes);//返回文件数据给客户端下载
            }
            catch
            {

            }
        }

        public bool IsReusable
        {
            get
            {
                return false;
            }
        }
    }
}