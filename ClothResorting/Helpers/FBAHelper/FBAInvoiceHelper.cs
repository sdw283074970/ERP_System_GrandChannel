using ClothResorting.Models;
using ClothResorting.Models.StaticClass;
using Microsoft.Office.Interop.Excel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Data.Entity;

namespace ClothResorting.Helpers.FBAHelper
{
    public class FBAInvoiceHelper
    {
        private ApplicationDbContext _context;
        private string _path = "";
        private _Application _excel;
        private Workbook _wb;
        private Worksheet _ws;
        private delegate void QuitHandler();

        public FBAInvoiceHelper()
        {
            _context = new ApplicationDbContext();
        }

        public FBAInvoiceHelper(string templatePath)
        {
            _context = new ApplicationDbContext();
            _path = templatePath;
            _excel = new Application();
            _wb = _excel.Workbooks.Open(_path);
        }

        //输入Invoice Detail列表，客户CODE，日期范围，生成Excel
        public string GenerateExcelPath(FBAInvoiceInfo info)
        {
            _ws = _wb.Worksheets[1];

            _ws.Cells[4, 2] = info.CustomerCode;
            _ws.Cells[4, 4] = info.FromDate == null ? "" : info.FromDate.ToString().Substring(0, 10);
            _ws.Cells[4, 6] = info.ToDate == null ? "" : info.ToDate.ToString().Substring(0, 10);
            _ws.Cells[4, 8] = DateTime.Now.ToString("yyyy-MM-dd");

            var startRow = 9;

            foreach (var i in info.InvoiceReportDetails)
            {
                _ws.Cells[startRow, 1] = i.InvoiceType;
                _ws.Cells[startRow, 2] = i.Reference;
                _ws.Cells[startRow, 3] = i.Activity;
                _ws.Cells[startRow, 4] = i.ChargingType;
                _ws.Cells[startRow, 5] = i.Unit;
                _ws.Cells[startRow, 6] = i.Quantity;
                _ws.Cells[startRow, 7] = i.Rate;
                _ws.Cells[startRow, 8] = i.Amount;
                _ws.Cells[startRow, 9] = i.DateOfCost.ToString("yyyy-MM-dd");
                _ws.Cells[startRow, 10] = i.Memo;

                startRow += 1;
            }

            _ws.Cells[startRow, 7] = "Total";
            _ws.Cells[startRow, 8] = info.InvoiceReportDetails.Sum(x => x.Amount);

            var fullPath = @"D:\ChargingReport\FBA-" + info.CustomerCode + "-ChargingReport-" + DateTime.Now.ToString("yyyyMMddhhmmssffff") + ".xls";
            _wb.SaveAs(fullPath, Type.Missing, "", "", Type.Missing, Type.Missing, XlSaveAsAccessMode.xlNoChange, 1, false, Type.Missing, Type.Missing, Type.Missing);

            _excel.Quit();

            var killer = new ExcelKiller();

            killer.Dispose();

            return fullPath;
        }

        public FBAInvoiceInfo GetChargingReportFormOrder(string reference, string invoiceType)
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

                if (masterOrderInDb.InvoiceStatus != "Closed")
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

                if (shipOrderInDb.InvoiceStatus != "Closed")
                    shipOrderInDb.InvoiceStatus = "Exported";
                info.InvoiceReportDetails = invoiceReportList;
            }

            _context.SaveChanges();

            return info;
        }

        public FBAInvoiceInfo GetChargingReportFormDateRangeAndCustomerId(int customerId, DateTime startDate, DateTime closeDate)
        {
            var customer = _context.UpperVendors.Find(customerId);

            var invoiceReportList = new List<InvoiceReportDetail>();

            var info = new FBAInvoiceInfo
            {
                FromDate = startDate,
                ToDate = closeDate,
                CustomerCode = customer.CustomerCode
            };

            var invoiceDetails = _context.InvoiceDetails
                .Include(x => x.FBAMasterOrder.Customer)
                .Include(x => x.FBAShipOrder)
                .Where(x => x.FBAMasterOrder.Customer.Id == customerId || x.FBAShipOrder.CustomerCode == customer.CustomerCode)
                .Where(x => x.DateOfCost >= startDate && x.DateOfCost <= closeDate)
                .ToList();

            foreach(var i in invoiceDetails)
            {
                var newInvoiceDetail = new InvoiceReportDetail
                {
                    InvoiceType = i.InvoiceType,
                    Activity = i.Activity,
                    ChargingType = i.ChargingType,
                    Unit = i.Unit,
                    Quantity = i.Quantity,
                    Rate = i.Rate,
                    Amount = i.Amount,
                    DateOfCost = i.DateOfCost,
                    Memo = i.Memo
                };

                if (i.FBAMasterOrder == null)
                {
                    newInvoiceDetail.Reference = i.FBAShipOrder.ShipOrderNumber;
                }
                else if (i.FBAShipOrder == null)
                {
                    newInvoiceDetail.Reference = i.FBAMasterOrder.Container;
                }

                invoiceReportList.Add(newInvoiceDetail);
            }

            info.InvoiceReportDetails = invoiceReportList;

            return info;
        }

        public UpperVendor GetCustomer(string reference, string invoiceType)
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

    public class FBAInvoiceInfo
    {
        public string CustomerCode { get; set; }

        public DateTime? FromDate { get; set; }

        public DateTime? ToDate { get; set; }

        public List<InvoiceReportDetail> InvoiceReportDetails { get; set; }
    }
    
    public class InvoiceReportDetail
    {
        public string Reference { get; set; }

        public string Activity { get; set; }

        public string InvoiceType { get; set; }

        public string ChargingType { get; set; }

        public string Unit { get; set; }

        public double Quantity { get; set; }

        public double Rate { get; set; }

        public double Amount { get; set; }

        public DateTime DateOfCost { get; set; }

        public string Memo { get; set; }
    }
}