using ClothResorting.Helpers;
using ClothResorting.Models;
using ClothResorting.Models.DataTransferModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;

namespace ClothResorting.Controllers.Api
{
    public class NewBulkloadController : ApiController
    {
        private ApplicationDbContext _context;

        public NewBulkloadController()
        {
            _context = new ApplicationDbContext();
        }

        // POST /api/thirdpartylogisticsapi
        [HttpPost]
        public void SaveUploadedFileAndExtractExcel()
        {
            var fileSavePath = "";

            //方法1：写入磁盘系统
            if (HttpContext.Current.Request.Files.AllKeys.Any())
            {
                var httpPostedFile = HttpContext.Current.Request.Files[0];

                if (httpPostedFile != null)
                {
                    fileSavePath = @"D:\TempFiles\" + httpPostedFile.FileName;

                    httpPostedFile.SaveAs(fileSavePath);
                }
            }

            //目前暂时无法处理同名文件，有空回来改

            var excel = new ExcelExtracter(fileSavePath);

            excel.ExtractBulkloadRecord();

            ////再次强行释放EXCEL资源(终止EXCEL进程)
            //excel.Dispose();
        }
    }
}
