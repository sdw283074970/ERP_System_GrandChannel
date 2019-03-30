using ClothResorting.Helpers;
using ClothResorting.Models.StaticClass;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;

namespace ClothResorting.Controllers.Api.Fba
{
    public class DownloadFileController : ApiController
    {
        // GET /api/fba/downloadfile/?fileName={fileName}
        [HttpGet]
        public IHttpActionResult DownloadFileWithinOperation([FromUri]string fileName)
        {
                var response = HttpContext.Current.Response;
                var downloadFile = new FileInfo(@"D:\BOL\" + fileName);
                response.ClearHeaders();
                response.Buffer = false;
                response.ContentType = "application/pdf";
                response.AppendHeader("Content-Disposition", "attachment; filename=" + HttpUtility.UrlEncode(fileName, System.Text.Encoding.UTF8));
                response.Clear();
                response.AppendHeader("Content-Length", downloadFile.Length.ToString());
                response.WriteFile(downloadFile.FullName);
                response.Flush();
                response.Close();
                response.End();

            return Ok();
        }

        // GET /api/fba/downloadfile/
        [HttpGet]
        public IHttpActionResult DownloadFile()
        {
            var fileName = DownloadRecord.FileName;
            var response = HttpContext.Current.Response;
            var downloadFile = new FileInfo(DownloadRecord.FilePath);
            response.ClearHeaders();
            response.Buffer = false;
            response.ContentType = "application/octet-stream";
            response.AppendHeader("Content-Disposition", "attachment; filename=" + HttpUtility.UrlEncode(fileName, System.Text.Encoding.UTF8));
            response.Clear();
            response.AppendHeader("Content-Length", downloadFile.Length.ToString());
            response.WriteFile(downloadFile.FullName);
            response.Flush();
            response.Close();
            response.End();

            return Ok();
        }

        // GET /api/downloadfile/?fullPath={fullPath}
        [HttpGet]
        public void DownloadByFullPath([FromUri]string fullPath)
        {
            var downloader = new Downloader();

            downloader.DownloadByFullPath(fullPath);
        }
    }
}
