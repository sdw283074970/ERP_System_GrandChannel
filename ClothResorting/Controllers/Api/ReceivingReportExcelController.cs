using ClothResorting.Helpers;
using ClothResorting.Models;
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
    public class ReceivingReportExcelController : ApiController
    {
        private ApplicationDbContext _context;

        public ReceivingReportExcelController()
        {
            _context = new ApplicationDbContext();
        }

        // GET /applicationExcel/?preid={preid}&container={container}
        [HttpGet]
        public IHttpActionResult DownloadExcel([FromUri]int preid, [FromUri]string container)
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
                    SKU = cartonDetail.SKU,
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

            var containerInDb = _context.Containers.SingleOrDefault(x => x.ContainerNumber == container);

            var generator = new ExcelGenerator();
            generator.GenerateRecevingReportExcel(containerInDb, resultList);

            return Ok(resultList);
        }
    }
}
