using ClothResorting.Models;
using ClothResorting.Models.StaticClass;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Data.Entity;
using ClothResorting.Helpers.FBAHelper;
using ClothResorting.Helpers;

namespace ClothResorting.Controllers.Api.Fba
{
    public class FBAReportDonwloadController : ApiController
    {
        private ApplicationDbContext _context;

        public FBAReportDonwloadController()
        {
            _context = new ApplicationDbContext();
        }

        // GET /api/fba/FBAReportdownload/?fullPath={fullPath}&prefix={prefix}&suffix={suffix}
        [HttpGet]
        public void DownloadChargingReport([FromUri]string fullPath, [FromUri]string prefix, [FromUri]string suffix)
        {
            var downloader = new Downloader();

            downloader.DownloadGeneralFileFromServer(fullPath, prefix, suffix);
        }

        // GET /api/fba/fbaReportdownload/?reference={reference}&invoiceType={invoiceType}
        [HttpGet]
        public IHttpActionResult DownloadChargingReportFormOrder([FromUri]string reference, [FromUri]string invoiceType)
        {
            var templatePath = @"D:\Template\FBA-InvoiceReport-Template.xls";

            var excelGenerator = new FBAInvoiceHelper(templatePath);

            var info = GetChargingReportFormOrder(reference, invoiceType);

            var path = excelGenerator.GenerateExcelPath(info);

            return Ok(path);
        }

        private FBAInvoiceInfo GetChargingReportFormOrder(string reference, string invoiceType)
        {
            var customer = GetCustomer(reference, invoiceType);
            var invoiceReportList = new List<InvoiceReportDetail>();
            var info = new FBAInvoiceInfo
            {
                FromDate = null,
                ToDate = null,
                CustomerCode = customer.CustomerCode
            };

            if (invoiceType == FBAInvoiceType.MasterOrder)
            {
                var masterOrderInDb = _context.FBAMasterOrders
                    .Include(x => x.InvoiceDetails)
                    .SingleOrDefault(x => x.Container == reference);

                var invoiceDetailList = masterOrderInDb.InvoiceDetails.ToList();

                foreach (var i in invoiceDetailList)
                {
                    invoiceReportList.Add(new InvoiceReportDetail
                    {
                        InvoiceType = i.InvoiceType,
                        Reference = reference,
                        Activity = i.Activity,
                        ChargingType = i.ChargingType,
                        Unit = i.Unit,
                        Quantity = i.Quantity,
                        Rate = i.Rate,
                        Amount = i.Amount,
                        DateOfCost = i.DateOfCost,
                        Memo = i.Memo
                    });
                }

                info.InvoiceReportDetails = invoiceReportList;

                masterOrderInDb.InvoiceStatus = "Exported";
            }
            else if (invoiceType == FBAInvoiceType.ShipOrder)
            {
                var shipOrderInDb = _context.FBAShipOrders
                    .Include(x => x.InvoiceDetails)
                    .SingleOrDefault(x => x.ShipOrderNumber == reference);

                var invoiceDetailList = shipOrderInDb.InvoiceDetails.ToList();

                foreach (var i in invoiceDetailList)
                {
                    invoiceReportList.Add(new InvoiceReportDetail
                    {
                        InvoiceType = i.InvoiceType,
                        Reference = reference,
                        Activity = i.Activity,
                        ChargingType = i.ChargingType,
                        Unit = i.Unit,
                        Quantity = i.Quantity,
                        Rate = i.Rate,
                        Amount = i.Amount,
                        DateOfCost = i.DateOfCost,
                        Memo = i.Memo
                    });
                }

                shipOrderInDb.InvoiceStatus = "Exported";
                info.InvoiceReportDetails = invoiceReportList;
            }

            _context.SaveChanges();

            return info;
        }

        private UpperVendor GetCustomer(string reference, string invoiceType)
        {
            UpperVendor customer = null;

            if (invoiceType == FBAInvoiceType.MasterOrder)
            {
                customer = _context.FBAMasterOrders
                    .Include(x => x.Customer)
                    .FirstOrDefault(x => x.Container == reference)
                    .Customer;
            }
            else if (invoiceType == FBAInvoiceType.ShipOrder)
            {
                var customerCode = _context.FBAShipOrders.SingleOrDefault(x => x.ShipOrderNumber == reference).CustomerCode;

                customer = _context.UpperVendors.SingleOrDefault(x => x.CustomerCode == customerCode);
            }

            return customer;
        }
    }
}
