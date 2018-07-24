﻿using ClothResorting.Models;
using ClothResorting.Models.DataTransferModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Data.Entity;

namespace ClothResorting.Controllers.Api
{
    public class FCReceivingReportController : ApiController
    {
        private ApplicationDbContext _context;

        public FCReceivingReportController()
        {
            _context = new ApplicationDbContext();
        }

        // GET /api/fcreceivingreport/?preid={preId}&container={container}
        [HttpGet]
        public IHttpActionResult GenerateReceivingReport([FromUri]int preid, [FromUri]string container)
        {
            var resultList = new List<FCReceivingReport>();

            var fcRegualrCartonDetailsInDb = _context.RegularCartonDetails
                .Include(c => c.POSummary.PreReceiveOrder)
                .Where(c => c.POSummary.PreReceiveOrder.Id == preid
                    && c.POSummary.Container == container)
                .ToList();

            foreach(var cartonDetail in fcRegualrCartonDetailsInDb)
            {
                var report = new FCReceivingReport
                {
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
                    Memo = ""
                };

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

            return Created(Request.RequestUri + "/" + resultList.Count, resultList);
        }

        // PUT /api/fcreceivingreport/{id}(preId) 在Purchase Order over view 页面中一键全收货，接口放在这里挤一挤
        [HttpPut]
        public void ReceiveAllPoWithoutProblem([FromUri]int id)
        {
            var cartonDetailsInDb = _context.RegularCartonDetails
                .Include(x => x.POSummary.PreReceiveOrder)
                .Where(x => x.POSummary.PreReceiveOrder.Id == id);

            foreach(var carton in cartonDetailsInDb)
            {
                carton.ActualCtns = carton.Cartons;
                carton.ActualPcs = carton.Quantity;
                carton.ToBeAllocatedCtns = carton.Cartons;
                carton.ToBeAllocatedPcs = carton.Quantity;

                carton.POSummary.ActualCtns = carton.POSummary.Cartons;
                carton.POSummary.ActualPcs = carton.POSummary.Quantity;

                carton.POSummary.PreReceiveOrder.ActualReceivedCtns = carton.POSummary.PreReceiveOrder.TotalCartons;
                carton.POSummary.PreReceiveOrder.ActualReceivedPcs = carton.POSummary.PreReceiveOrder.TotalPcs;
            }

            _context.SaveChanges();
        }
    }
}
