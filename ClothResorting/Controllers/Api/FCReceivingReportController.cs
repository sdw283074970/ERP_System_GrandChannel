using ClothResorting.Models;
using ClothResorting.Models.DataTransferModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Data.Entity;
using ClothResorting.Helpers;
using System.Web;

namespace ClothResorting.Controllers.Api
{
    public class FCReceivingReportController : ApiController
    {
        private ApplicationDbContext _context;
        private string _userName;

        public FCReceivingReportController()
        {
            _context = new ApplicationDbContext();
            _userName = HttpContext.Current.User.Identity.Name.Split('@')[0] == "" ? (HttpContext.Current.Request.Headers.Get("AppUser") == null ? "" : HttpContext.Current.Request.Headers.Get("AppUser").Split('@')[0]) : HttpContext.Current.User.Identity.Name.Split('@')[0];
        }

        // GET /api/fcreceivingreport/?preid={preId}&container={container}
        [HttpGet]
        public IHttpActionResult GenerateReceivingReport([FromUri]int preid, [FromUri]string container)
        {
            return Ok(GetFCReceivingReportList(preid, container));
        }
        // PUT /api/fcreceivingreport/{id}(preId) 在Purchase Order over view 页面中一键全收货，接口放在这里挤一挤
        [HttpPut]
        public void ReceiveAllPoWithoutProblem([FromUri]int id)
        {
            var cartonDetailsInDb = _context.RegularCartonDetails
                .Include(x => x.POSummary.PreReceiveOrder)
                .Where(x => x.POSummary.PreReceiveOrder.Id == id);

            int? actualCtns = 0;
            int? actualPcs = 0;

            foreach(var carton in cartonDetailsInDb)
            {
                //批量收获只对实收货物间数仍然为0的有效收货对象有效，防止多次点击生成额外的待分配件数
                if ((carton.Quantity != 0 && carton.ActualPcs == 0) || (carton.Cartons != 0 && carton.ActualCtns == 0))
                {
                    //更新收货人
                    carton.Receiver = _userName;

                    carton.Status = "To Be Allocated";

                    carton.ActualCtns = carton.Cartons;
                    carton.ActualPcs = carton.Quantity;
                    carton.ToBeAllocatedCtns = carton.Cartons;
                    carton.ToBeAllocatedPcs = carton.Quantity;

                    actualCtns += carton.Cartons;
                    actualPcs += carton.Quantity;

                }
            }

            _context.SaveChanges();
        }

        // DELETE /api/fcreceivingreport/{id}(poId) 删除PO的接口，没其他位置了放这里挤一挤
        [HttpDelete]
        public void DeleteBulkPo([FromUri]int id)
        {
            var cartonDetails = _context.RegularCartonDetails
                .Include(x => x.POSummary.PreReceiveOrder)
                .Where(x => x.POSummary.Id == id);

            foreach(var cartonDetail in cartonDetails)
            {
                cartonDetail.POSummary.PreReceiveOrder.ActualReceivedCtns -= cartonDetail.ActualCtns;
                cartonDetail.POSummary.PreReceiveOrder.ActualReceivedPcs -= cartonDetail.ActualPcs;
                cartonDetail.POSummary.PreReceiveOrder.TotalCartons -= cartonDetail.Cartons;
                cartonDetail.POSummary.PreReceiveOrder.TotalPcs -= cartonDetail.Quantity;
            }

            try
            {
                _context.RegularCartonDetails.RemoveRange(cartonDetails);
                _context.POSummaries.Remove(_context.POSummaries.Find(id));
                _context.SaveChanges();
            }
            catch(Exception e)
            {
                throw new Exception("Cannot delete this PO Summary, because other Location may rely on it.");
            }
        }

        //私有方法
        private IList<FCReceivingReport> GetFCReceivingReportList(int preid, string container)
        {
            var resultList = new List<FCReceivingReport>();

            var fcRegualrCartonDetailsInDb = _context.RegularCartonDetails
                .Include(c => c.POSummary.PreReceiveOrder)
                .Where(c => c.POSummary.PreReceiveOrder.Id == preid
                    && c.POSummary.Container == container)
                .OrderBy(x => x.PurchaseOrder)
                .ToList();

            var index = 1;

            foreach (var cartonDetail in fcRegualrCartonDetailsInDb)
            {
                var report = new FCReceivingReport
                {
                    Index = index,
                    CartonRange = cartonDetail.CartonRange,
                    PurchaseOrder = cartonDetail.PurchaseOrder,
                    Style = cartonDetail.Style,
                    Line = cartonDetail.POSummary.PoLine,
                    Customer = cartonDetail.Customer,
                    SizeBundle = cartonDetail.SizeBundle,
                    PcsBundle = cartonDetail.PcsBundle,
                    ReceivableQty = cartonDetail.Quantity,
                    ReceivedQty = cartonDetail.ActualPcs,
                    ReceivableCtns = cartonDetail.Cartons,
                    ReceivedCtns = cartonDetail.ActualCtns,
                    Color = cartonDetail.Color,
                    SKU = cartonDetail.Batch,
                    Memo = "",
                    Comment = cartonDetail.Comment
                };

                index++;

                if (report.ReceivedCtns - report.ReceivableCtns < 0)
                {
                    var diff = report.ReceivableCtns - report.ReceivedCtns;
                    report.Memo = "Shortage: " + diff.ToString() + "ctns";
                }

                if (report.ReceivedCtns - report.ReceivableCtns > 0)
                {
                    var diff = report.ReceivedCtns - report.ReceivableCtns;
                    report.Memo = "Overage: " + diff.ToString() + "ctns";
                }

                resultList.Add(report);
            }

            return resultList;
        }
    }
}
