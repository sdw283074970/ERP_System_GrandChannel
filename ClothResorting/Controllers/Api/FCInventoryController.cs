using AutoMapper;
using ClothResorting.Dtos;
using ClothResorting.Helpers;
using ClothResorting.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace ClothResorting.Controllers.Api
{
    public class FCInventoryController : ApiController
    {
        private ApplicationDbContext _context;

        public FCInventoryController()
        {
            _context = new ApplicationDbContext();
        }

        // POST /api/freecountryinventory
        [HttpPost]
        public void UploadAndExtractFreeCountryExcel()
        {
            var fileSavePath = "";

            //从httpRequest中获取文件并写入磁盘系统
            var filesGetter = new FilesGetter();

            fileSavePath = filesGetter.GetAndSaveFileFromHttpRequest(@"D:\TempFiles\");

            if (fileSavePath == "")
            {
                throw new HttpResponseException(HttpStatusCode.BadRequest);
            }

            var excel = new ExcelExtracter(fileSavePath);

            excel.CreateFCPreReceiveOrder();

            excel.ExtractFCPurchaseOrderSummary();

            excel.ExtractFCPurchaseOrderDetail();

            //强行关闭进程
            var killer = new ExcelKiller();

            killer.Dispose();
        }

        // GET /api/freecountryinventory/{id} 以id查找并返回preReceiveOrder中的所有PO
        public IHttpActionResult GetPurchaseOrderDetail(int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            var purchaseOrderDetails = _context.POSummaries
                .Where(s => s.PreReceiveOrder.Id == id)
                .Select(Mapper.Map<POSummary, POSummaryDto>);

            return Ok(purchaseOrderDetails);
        }
    }
}
