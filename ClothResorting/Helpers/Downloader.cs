using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace ClothResorting.Helpers
{
    public class Downloader
    {
        public void DownloadFromServer(string fullFileName, string rootPath)
        {
            var fullPath = rootPath + fullFileName;
            var response = HttpContext.Current.Response;
            var downloadFile = new FileInfo(fullPath);
            response.ClearHeaders();
            response.Buffer = false;
            response.ContentType = "application/octet-stream";
            response.AppendHeader("Content-Disposition", "attachment; filename=" + HttpUtility.UrlEncode(fullFileName, System.Text.Encoding.UTF8));
            response.Clear();
            response.AppendHeader("Content-Length", downloadFile.Length.ToString());
            response.WriteFile(downloadFile.FullName);
            response.Flush();
            response.Close();
            response.End();
        }
    }
}