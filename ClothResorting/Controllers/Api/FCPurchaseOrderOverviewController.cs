using ClothResorting.Models;
using ClothResorting.Models.ApiTransformModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Data.Entity;
using AutoMapper;
using ClothResorting.Dtos;
using ClothResorting.Helpers;

namespace ClothResorting.Controllers.Api
{
    public class FCPurchaseOrderOverviewController : ApiController
    {
        private ApplicationDbContext _context;

        public FCPurchaseOrderOverviewController()
        {
            _context = new ApplicationDbContext();
        }

        // GET /api/fcpurchaseorderoverview/{id} 以id查找并返回preReceiveOrder中的所有PO
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

        // POST /api/fcpurchaseorderoverview
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

        // PUT /api/fcpurchaseorderoverview 更新pl的container信息
        [HttpPut]
        public void UpdateContainer([FromBody]ArrPreIdContainerJsonObj obj)
        {
            var arr = obj.Arr;
            var container = obj.Container;
            var preId = obj.PreId;

            var poSummariesInDb = _context.POSummaries
                .Include(c => c.PreReceiveOrder)
                .Include(c => c.RegularCartonDetails)
                .Where(c => c.PreReceiveOrder.Id == preId);

            foreach(var id in arr)
            {
                poSummariesInDb.SingleOrDefault(c => c.Id == id).Container = container;

                foreach(var carton in poSummariesInDb.SingleOrDefault(c => c.Id == id).RegularCartonDetails)
                {
                    carton.Container = container;
                }
            }
            _context.SaveChanges();
        }
    }
}
