using ClothResorting.Models;
using ClothResorting.Models.StaticClass;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Data.Entity;
using static org.in2bits.MyOle2.Metadata.Property;

namespace ClothResorting.Controllers.Api
{
    public class OnlineFixController : ApiController
    {
        private ApplicationDbContext _context;

        public OnlineFixController()
        {
            _context = new ApplicationDbContext();
        }

        // PUT /api/onlinefix/
        [HttpPut]
        public IHttpActionResult FixProblems()
        {
            //var workOrdersInDb = _context.PreReceiveOrders
            //    .Include(x => x.UpperVendor)
            //    .Where(x => x.Id > 0);

            //var fc = _context.UpperVendors.Find(2);

            //foreach(var w in workOrdersInDb)
            //{
            //    w.UpperVendor = fc;
            //    w.CustomerName = Vendor.FreeCountry;
            //}

            //var poSummariesInDb = _context.POSummaries
            //    .Include(x => x.RegularCartonDetails)
            //    .Where(x => x.Id > 0);

            //foreach (var p in poSummariesInDb)
            //{
            //    p.Vendor = Vendor.FreeCountry;

            //    if (p.RegularCartonDetails.Count == 1)
            //    {
            //        p.OrderType = OrderType.Prepack;
            //    }
            //    else
            //    {
            //        p.OrderType = OrderType.SolidPack;
            //    }
            //}

            //_context.SaveChanges();

            //var cartonDetailsInDb = _context.RegularCartonDetails
            //    .Include(x => x.POSummary)
            //    .Where(x => x.Id > 0);

            //foreach (var c in cartonDetailsInDb)
            //{
            //    c.OrderType = c.POSummary.OrderType;
            //}

            //_context.SaveChanges();

            //修复删除重复的POSummary

            //var preId = 310;

            //var poSummariesInDb = _context.POSummaries
            //    .Include(x => x.PreReceiveOrder)
            //    .Where(x => x.PreReceiveOrder.Id == preId)
            //    .ToList();

            //var repetitveIdList = new List<int>();

            //for (int i = 0; i < poSummariesInDb.Count() - 1; i++)
            //{
            //    for (int j = i + 1; j < poSummariesInDb.Count(); j++)
            //    {
            //        if (poSummariesInDb[j].Quantity == poSummariesInDb[i].Quantity && poSummariesInDb[j].Style == poSummariesInDb[i].Style && poSummariesInDb[j].PurchaseOrder == poSummariesInDb[i].PurchaseOrder && poSummariesInDb[j].Cartons == poSummariesInDb[i].Cartons)
            //        {
            //            if (!repetitveIdList.Contains(poSummariesInDb[j].Id))
            //            {
            //                repetitveIdList.Add(poSummariesInDb[j].Id);
            //            }
            //        }
            //    }
            //}

            //foreach (var id in repetitveIdList)
            //{
            //    _context.FCRegularLocationDetails.RemoveRange(_context.FCRegularLocationDetails
            //        .Include(x => x.RegularCaronDetail.POSummary)
            //        .Where(x => x.RegularCaronDetail.POSummary.Id == id));

            //    _context.RegularCartonDetails.RemoveRange(_context.RegularCartonDetails
            //        .Include(x => x.POSummary)
            //        .Where(x => x.POSummary.Id == id));

            //    _context.POSummaries.Remove(_context.POSummaries.Find(id));
            //}

            //_context.SaveChanges();

            //同时修复应收/实收箱数件数

            var preOrderInDb = _context.PreReceiveOrders
                .Include(x => x.POSummaries)
                .Where(x => x.Id > 0);

            foreach (var preOrder in preOrderInDb)
            {
                preOrder.TotalCartons = preOrder.POSummaries.Sum(x => x.Cartons);
                preOrder.TotalPcs = preOrder.POSummaries.Sum(x => x.Quantity);

                preOrder.ActualReceivedCtns = preOrder.POSummaries.Sum(x => x.ActualCtns);
                preOrder.ActualReceivedPcs = preOrder.POSummaries.Sum(x => x.ActualPcs);
            }

            _context.SaveChanges();

            //修复所有的LocationDetail中的Batch

            //var locationInDb = _context.FCRegularLocationDetails
            //    .Include(x => x.RegularCaronDetail)
            //    .Where(x => x.Id > 0);

            //foreach(var location in locationInDb)
            //{
            //    location.Batch = location.RegularCaronDetail.Batch;
            //}

            //_context.SaveChanges();

            var cartonDetailInDb = _context.RegularCartonDetails
                .Include(x => x.FCRegularLocationDetail)
                .Where(x => x.Container == "STRONGHOLD FONTANA TRANSFER");

            var resultList = new List<int>();
            var result = "";

            foreach(var carton in cartonDetailInDb)
            {
                if (carton.FCRegularLocationDetail.Count == 0)
                {
                    result = result + " " + carton.Id.ToString();
                }
            }

            return Ok(result);
        }

        // DELETE /onlinefix/
        [HttpDelete]
        public void DeleteRepetitiveRecords()
        {
            var preId = 310;

            var poSummariesInDb = _context.POSummaries
                .Include(x => x.PreReceiveOrder)
                .Where(x => x.PreReceiveOrder.Id == preId)
                .ToList();

            var repetitveIdList = new List<int>();

            for (int i = 0; i < poSummariesInDb.Count() - 1; i++)
            {
                for (int j = i + 1; j < poSummariesInDb.Count() - 1; j++)
                {
                    if (poSummariesInDb[j].Quantity == poSummariesInDb[i].Quantity && poSummariesInDb[j].Style == poSummariesInDb[i].Style && poSummariesInDb[j].PurchaseOrder == poSummariesInDb[i].PurchaseOrder && poSummariesInDb[j].Cartons == poSummariesInDb[i].Cartons)
                    {
                        if (!repetitveIdList.Contains(poSummariesInDb[j].Id))
                        {
                            repetitveIdList.Add(poSummariesInDb[j].Id);
                        }
                    }
                }
            }


        }
    }
}
