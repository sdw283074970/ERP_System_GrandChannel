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

        // POST /api/fcpurchaseorderoverview/{id}
        [HttpPost]
        public void UploadAndExtractFreeCountryExcel([FromUri]int id)
        {
            //从httpRequest中获取文件并写入磁盘系统
            var filesGetter = new FilesGetter();

            var fileSavePath = filesGetter.GetAndSaveFileFromHttpRequest(@"D:\TempFiles\");

            if (fileSavePath == "")
            {
                throw new HttpResponseException(HttpStatusCode.BadRequest);
            }

            var excel = new ExcelExtracter(fileSavePath);

            excel.ExtractFCPurchaseOrderSummary(id);

            excel.ExtractFCPurchaseOrderDetail(id);

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

            //统计该PackingList下Container号码的数量，并将结果同步至prereceiveOrder中
            poSummariesInDb = _context.POSummaries
                .Include(c => c.PreReceiveOrder)
                .Include(c => c.RegularCartonDetails)
                .Where(c => c.PreReceiveOrder.Id == preId);

            var containerList = new List<string>();

            foreach(var poSummary in poSummariesInDb)
            {
                if (!containerList.Contains(poSummary.Container))
                {
                    containerList.Add(poSummary.Container);
                }
            }

            var containerString = containerList[0];

            for(int i = 1; i < containerList.Count; i++)
            {
                containerString += ", " + containerList[i];
            }

            var preReceiveOrderInDb = _context.PreReceiveOrders.Find(preId);

            preReceiveOrderInDb.ContainerNumber = containerString;

            _context.SaveChanges();
        }

        //单个删除主页中的FC相关的Prereceiverder记录
        // DELETE /api/fcpurchaseOrderOverview/
        [HttpDelete]
        public void DeleteFCPreReceiveOrder([FromUri]int id)
        {
            var pickingRecordsInDb = _context.PickingRecords
                .Include(c => c.FCRegularLocationDetail.PreReceiveOrder)
                .Where(c => c.FCRegularLocationDetail.PreReceiveOrder.Id == id);

            _context.PickingRecords.RemoveRange(pickingRecordsInDb);

            var cartonInsidesInDb = _context.CartonInsides
                .Include(c => c.FCRegularLocationDetail.PreReceiveOrder)
                .Where(c => c.FCRegularLocationDetail.PreReceiveOrder.Id == id);

            _context.CartonInsides.RemoveRange(cartonInsidesInDb);

            var pickingListsInDb = _context.PickingLists
                .Include(c => c.PreReceiveOrder)
                .Where(c => c.PreReceiveOrder.Id == id);

            _context.PickingLists.RemoveRange(pickingListsInDb);

            var fcRegularLocationDetialsInDb = _context.FCRegularLocationDetails
                .Include(c => c.PreReceiveOrder)
                .Where(c => c.PreReceiveOrder.Id == id);

            _context.FCRegularLocationDetails.RemoveRange(fcRegularLocationDetialsInDb);

            var regularCartonDetailsInDb = _context.RegularCartonDetails
                .Include(x => x.POSummary.PreReceiveOrder)
                .Where(x => x.POSummary.PreReceiveOrder.Id == id);

            _context.RegularCartonDetails.RemoveRange(regularCartonDetailsInDb);

            var poSummariesInDb = _context.POSummaries
                .Include(x => x.PreReceiveOrder)
                .Where(x => x.PreReceiveOrder.Id == id);

            _context.POSummaries.RemoveRange(poSummariesInDb);

            var preReceiveOrderInDb = _context.PreReceiveOrders.Find(id);

            _context.PreReceiveOrders.Remove(preReceiveOrderInDb);

            _context.SaveChanges();
        }
    }
}
