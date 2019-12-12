using ClothResorting.Helpers;
using ClothResorting.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using System.Data.Entity;
using ClothResorting.Models.StaticClass;

namespace ClothResorting.Controllers.Api
{
    public class ThirdPartyLogisticsApiController : ApiController
    {
        private ApplicationDbContext _context;

        public ThirdPartyLogisticsApiController()
        {
            _context = new ApplicationDbContext();
        }

        // POST /api/thirdpartylogisticsapi/{id}
        [HttpPost]
        public void SaveUploadedFileAndExtractExcel([FromUri]int id, [FromUri]string vendor)
        {
            var fileSavePath = "";

            //写入磁盘系统
            var filesGetter = new FilesGetter();

            fileSavePath = filesGetter.GetAndSaveSingleFileFromHttpRequest(@"D:\TempFiles\");

            if (fileSavePath == "")
            {
                throw new HttpResponseException(HttpStatusCode.BadRequest);
            }

            var excel = new ExcelExtracter(fileSavePath);

            excel.ExtractPOSummaryAndCartonDetail(id, vendor);

            //强行关闭进程
            var killer = new ExcelKiller();

            killer.Dispose();
        }
    }
}
