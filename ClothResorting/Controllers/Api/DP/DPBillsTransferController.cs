using ClothResorting.Helpers;
using ClothResorting.Helpers.DPHelper;
using ICSharpCode.SharpZipLib.Zip;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;

namespace ClothResorting.Controllers.Api.DP
{
    public class DPBillsTransferController : ApiController
    {
        // POST /api/DP/DPBillsTransfer/
        [HttpPost]
        public void DownloadTransferedBills()
        {
            var targetRootPath = @"E:\TempFiles\";
            var pathList = new List<string>();
            var billCleaner = new BillCleaner();
            var zipper = new ZipperNameTransform();
            var zipFilePath = targetRootPath + DateTime.Now.ToString("yyyyMMddhhMmss") + "_Bills.zip";
            var downloader = new Downloader();

            if (HttpContext.Current.Request.Files.AllKeys.Any())
            {
                for (var i = 0; i < HttpContext.Current.Request.Files.Count; i++)
                {
                    var httpPostedFile = HttpContext.Current.Request.Files[i];

                    if (httpPostedFile != null)
                    {
                        var timeStamp = DateTime.Now.Year.ToString()
                            + DateTime.Now.Month.ToString()
                            + DateTime.Now.Day.ToString()
                            + DateTime.Now.Hour.ToString()
                            + DateTime.Now.Second.ToString()
                            + DateTime.Now.Millisecond.ToString();

                        string fileNameOnly = httpPostedFile.FileName.Split('\\').Last();
                        //var FileName = fileNameOnly;
                        var filePath = targetRootPath + timeStamp + " - " + fileNameOnly;
                        httpPostedFile.SaveAs(filePath);
                        var finalPath = billCleaner.ClearBills(filePath);
                        pathList.Add(finalPath);
                    }
                }
            }

            var downloadPath = zipper.DownloadZippedFile(pathList.ToArray(), zipFilePath, HttpContext.Current);
            downloader.DownloadByFullPath(downloadPath);
        }
    }
}
