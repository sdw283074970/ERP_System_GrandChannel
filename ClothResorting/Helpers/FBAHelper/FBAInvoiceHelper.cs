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
        public string GenerateExcelFileAndReturnPath(FBAInvoiceInfo info)
        {
            //Worksheet summaryWookSheet;
            //summaryWookSheet = (Worksheet)_wb.Worksheets.Add();

            for(int i = 1; i <= 3; i++)
            {
                _ws = _wb.Worksheets[i];
                _ws.Cells[4, 2] = info.CustomerCode;
                _ws.Cells[4, 4] = info.FromDate == null ? "" : info.FromDate.ToString("yyyy-MM-dd").Substring(0, 10);
                _ws.Cells[4, 6] = info.ToDate == null ? "" : info.ToDate.ToString("yyyy-MM-dd").Substring(0, 10);
                _ws.Cells[4, 8] = DateTime.Now.ToString("yyyy-MM-dd");
            }

            //制作第一个Summary表
            _ws = _wb.Worksheets[1];

            var startRow = 9;

            foreach (var i in info.InvoiceReportDetails)
            {
                _ws.Cells[startRow, 1] = i.InvoiceType;
                _ws.Cells[startRow, 2] = i.Reference;
                _ws.Cells[startRow, 3] = i.GrandNumber;
                _ws.Cells[startRow, 4] = i.SubCustomer ?? "N/A";
                _ws.Cells[startRow, 5] = i.Activity;
                _ws.Cells[startRow, 6] = i.ChargingType;
                _ws.Cells[startRow, 7] = i.Unit;
                _ws.Cells[startRow, 8] = i.Quantity;
                _ws.Cells[startRow, 9] = i.Rate;
                _ws.Cells[startRow, 10] = i.Amount;
                _ws.Cells[startRow, 11] = i.Cost;
                _ws.Cells[startRow, 12] = i.DateOfCost.ToString("yyyy-MM-dd");
                _ws.Cells[startRow, 13] = i.Memo;

                startRow += 1;
            }

            _ws.Cells[startRow, 9] = "Total";
            _ws.Cells[startRow, 10] = info.InvoiceReportDetails.Sum(x => x.Amount);
            _ws.Cells[startRow, 11] = info.InvoiceReportDetails.Sum(x => x.Cost);

            //制作第二个收费项目统计表
            _ws = _wb.Worksheets[2];

            startRow = 8;

            var referenceGroup = info.InvoiceReportDetails.GroupBy(x => x.Reference);
            var chargeActivityGroup = info.InvoiceReportDetails.GroupBy(x => x.Activity);

            _ws.Cells[startRow, 1] = "Order Type";
            _ws.Cells[startRow, 2] = "Reference #";
            _ws.Cells[startRow, 3] = "Grand #";
            _ws.Cells[startRow, 4] = "Sub-customer";
            _ws.Cells[startRow, 5] = "Destination";
            _ws.Cells[startRow, 6] = "Total Ctns";
            _ws.Cells[startRow, 7] = "Total Plts";

            var columnIndex = 8;
            var activityList = new List<string>();

            foreach(var c in chargeActivityGroup)
            {
                _ws.Cells[startRow, columnIndex] = c.First().Activity;
                activityList.Add(c.First().Activity);
                columnIndex += 1;
            }

            _ws.Cells[startRow, columnIndex] = "Date of Close";
            _ws.Cells[startRow, columnIndex + 1] = "Amount";
            _ws.Cells[startRow, columnIndex + 2] = "Cost";

            startRow += 1;
            var countOfActivity = chargeActivityGroup.Count();
            var totalCtns = 0;
            var totalPlts = 0;

            foreach (var r in referenceGroup)
            {

                _ws.Cells[startRow, 1] = r.First().InvoiceType;
                _ws.Cells[startRow, 2] = r.First().Reference;
                _ws.Cells[startRow, 3] = r.First().GrandNumber;
                _ws.Cells[startRow, 4] = r.First().SubCustomer ?? "N/A";
                _ws.Cells[startRow, 5] = r.First().Destination;
                _ws.Cells[startRow, 6] = r.First().ActualCtnsInThisOrder;
                _ws.Cells[startRow, 7] = r.First().ActualPltsInThisOrder;
            
                for (var i = 0; i < countOfActivity; i++)
                {
                    _ws.Cells[startRow, 8 + i] = 0.0;
                }

                _ws.Cells[startRow, columnIndex] = r.First().DateOfClose.Year == 1900 ? "Open" : r.First().DateOfClose.ToString("MM/dd/yyyy");
                _ws.Cells[startRow, columnIndex + 1] = r.Sum(x => x.Amount);
                _ws.Cells[startRow, columnIndex + 2] = r.Sum(x => x.Cost);

                foreach (var i in r)
                {
                    var index = activityList.IndexOf(i.Activity);
                    _ws.Cells[startRow, index + 8] = Math.Round(_ws.Cells[startRow, index + 8].Value2 + i.Amount, 2);
                    
                }

                totalCtns += r.First().ActualCtnsInThisOrder;
                totalPlts += r.First().ActualPltsInThisOrder;

                startRow += 1;
            }

            foreach(var c in chargeActivityGroup)
            {
                var activity = c.First().Activity;
                _ws.Cells[startRow, activityList.IndexOf(activity) + 8] = Math.Round(info.InvoiceReportDetails.Where(x => x.Activity == activity).Sum(x => x.Amount), 2);
            }

            _ws.Cells[startRow, 1] = "Total";
            _ws.Cells[startRow, 6] = totalCtns;
            _ws.Cells[startRow, 7] = totalPlts;
            _ws.Cells[startRow, columnIndex + 1] = info.InvoiceReportDetails.Sum(x => x.Amount);
            _ws.Cells[startRow, columnIndex + 2] = info.InvoiceReportDetails.Sum(x => x.Cost);

            //制作第三个收费细节表
            _ws = _wb.Worksheets[3];

            startRow = 6;

            var shipOrderList = _context.FBAShipOrders
                .Include(x => x.FBAPickDetails)
                .Where(x => x.CustomerCode == info.CustomerCode
                    && x.ShipDate >= info.FromDate
                    && x.ShipDate <= info.ToDate)
                .ToList();

            foreach(var s in shipOrderList)
            {
                _ws.Cells[startRow, 1] = "Reference";
                _ws.Cells[startRow, 2] = s.ShipOrderNumber;
                _ws.Cells[startRow, 3] = "Outbound Date";
                _ws.Cells[startRow, 4] = s.ShipDate.ToString("yyyy-MM-dd");

                startRow++;

                _ws.Cells[startRow, 1] = "Container";
                _ws.Cells[startRow, 2] = "SKU";
                _ws.Cells[startRow, 3] = "Pickable Ctns";
                _ws.Cells[startRow, 4] = "Actual Ctns";
                _ws.Cells[startRow, 5] = "Plts From Inventory";
                _ws.Cells[startRow, 6] = "New Plts";
                _ws.Cells[startRow, 7] = "Actual Plts";
                _ws.Cells[startRow, 8] = "Inbound Date";
                _ws.Cells[startRow, 9] = "Ship Date";

                startRow++;

                foreach(var p in s.FBAPickDetails)
                {
                    _ws.Cells[startRow, 1] = p.Container;
                    _ws.Cells[startRow, 2] = p.ShipmentId;
                    _ws.Cells[startRow, 3] = p.PickableCtns;
                    _ws.Cells[startRow, 4] = p.ActualQuantity;
                    _ws.Cells[startRow, 5] = p.PltsFromInventory;
                    _ws.Cells[startRow, 6] = p.NewPlts;
                    _ws.Cells[startRow, 7] = p.ActualPlts;
                    _ws.Cells[startRow, 8] = p.InboundDate.ToString("yyyy-MM-dd");
                    _ws.Cells[startRow, 9] = p.FBAShipOrder.ShipDate.ToString("yyyy-MM-dd");

                    startRow++;
                }

                _ws.Cells[startRow, 1] = "Total";
                _ws.Cells[startRow, 3] = s.FBAPickDetails.Sum(x => x.PickableCtns);
                _ws.Cells[startRow, 4] = s.FBAPickDetails.Sum(x => x.ActualQuantity);
                _ws.Cells[startRow, 5] = s.FBAPickDetails.Sum(x => x.PltsFromInventory);
                _ws.Cells[startRow, 6] = s.FBAPickDetails.Sum(x => x.NewPlts);
                _ws.Cells[startRow, 7] = s.FBAPickDetails.Sum(x => x.ActualPlts);

                startRow += 2;
            }

            var fullPath = @"D:\ChargingReport\FBA-" + info.CustomerCode + "-ChargingReport-" + DateTime.Now.ToString("yyyyMMddhhmmssffff") + ".xls";
            _wb.SaveAs(fullPath, Type.Missing, "", "", Type.Missing, Type.Missing, XlSaveAsAccessMode.xlNoChange, 1, false, Type.Missing, Type.Missing, Type.Missing);

            _excel.Quit();

            var killer = new ExcelKiller();

            killer.Dispose();

            return fullPath;
        }

        public string GenerateExcelFileForAllCustomerAndReturnPath(FBAInvoiceInfo info)
        {
            _ws = _wb.Worksheets[1];

            _ws.Cells[4, 2] = info.CustomerCode;
            _ws.Cells[4, 4] = info.FromDate == null ? "" : info.FromDate.ToString("yyyy-MM-dd").Substring(0, 10);
            _ws.Cells[4, 6] = info.ToDate == null ? "" : info.ToDate.ToString("yyyy-MM-dd").Substring(0, 10);
            _ws.Cells[4, 8] = DateTime.Now.ToString("yyyy-MM-dd");

            var groupByCustomer = info.InvoiceReportDetails.GroupBy(x => x.CustomerCode);
            var startRow = 8;

            foreach (var g in groupByCustomer)
            {
                _ws.Cells[startRow - 1, 1] = g.First().CustomerCode;
                _ws.Cells[startRow, 1] = "Order Type";
                _ws.Cells[startRow, 2] = "Reference #";
                _ws.Cells[startRow, 3] = "Grand #";
                _ws.Cells[startRow, 4] = "Activity";
                _ws.Cells[startRow, 5] = "Charging Type";
                _ws.Cells[startRow, 6] = "UOM";
                _ws.Cells[startRow, 7] = "Quantity";
                _ws.Cells[startRow, 8] = "Rate";
                _ws.Cells[startRow, 9] = "Amout";
                _ws.Cells[startRow, 10] = "Date of Cost";
                _ws.Cells[startRow, 11] = "Memo";
                _ws.Cells[startRow, 12] = "Cost";

                startRow += 1;

                foreach (var i in g)
                {
                    _ws.Cells[startRow, 1] = i.InvoiceType;
                    _ws.Cells[startRow, 2] = i.Reference;
                    _ws.Cells[startRow, 3] = i.GrandNumber;
                    _ws.Cells[startRow, 4] = i.Activity;
                    _ws.Cells[startRow, 5] = i.ChargingType;
                    _ws.Cells[startRow, 6] = i.Unit;
                    _ws.Cells[startRow, 7] = i.Quantity;
                    _ws.Cells[startRow, 8] = i.Rate;
                    _ws.Cells[startRow, 9] = i.Amount;
                    _ws.Cells[startRow, 10] = i.DateOfCost.ToString("yyyy-MM-dd");
                    _ws.Cells[startRow, 11] = i.Memo;
                    _ws.Cells[startRow, 12] = i.Cost;

                    startRow += 1;
                }
                _ws.Cells[startRow, 8] = "Total";
                _ws.Cells[startRow, 9] = g.Sum(x => x.Amount);
                _ws.Cells[startRow, 12] = g.Sum(x => x.Cost);

                startRow += 3;
            }

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
                FromDate = new DateTime(1900, 1, 1, 0, 0, 0, 0),
                ToDate = new DateTime(1900, 1, 1, 0, 0, 0, 0),
                CustomerCode = customer.CustomerCode
            };

            if (invoiceType == FBAInvoiceType.MasterOrder)
            {
                var masterOrderInDb = _context.FBAMasterOrders
                    .Include(x => x.FBAOrderDetails)
                    .Include(x => x.InvoiceDetails)
                    .Include(x => x.FBAPallets)
                    .SingleOrDefault(x => x.Container == reference);

                var invoiceDetailList = masterOrderInDb.InvoiceDetails.ToList();

                foreach (var i in invoiceDetailList)
                {
                    invoiceReportList.Add(new InvoiceReportDetail
                    {
                        Cost = i.Cost,
                        InvoiceType = i.InvoiceType,
                        Reference = reference,
                        Activity = i.Activity,
                        SubCustomer = i.FBAMasterOrder.SubCustomer,
                        ChargingType = i.ChargingType,
                        Unit = i.Unit,
                        Quantity = i.Quantity,
                        Rate = i.Rate,
                        Amount = i.Amount,
                        DateOfCost = i.DateOfCost,
                        Memo = i.Memo,
                        ActualCtnsInThisOrder = masterOrderInDb.FBAOrderDetails.Sum(x => x.ActualQuantity),
                        ActualPltsInThisOrder = masterOrderInDb.FBAPallets.Sum(x => x.ActualPallets),
                        DateOfClose = masterOrderInDb.CloseDate
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
                    .Include(x => x.FBAPickDetails)
                    .SingleOrDefault(x => x.ShipOrderNumber == reference);

                var invoiceDetailList = shipOrderInDb.InvoiceDetails.ToList();

                foreach (var i in invoiceDetailList)
                {
                    invoiceReportList.Add(new InvoiceReportDetail
                    {
                        InvoiceType = i.InvoiceType,
                        Reference = reference,
                        SubCustomer = i.FBAShipOrder.SubCustomer,
                        Activity = i.Activity,
                        ChargingType = i.ChargingType,
                        Unit = i.Unit,
                        Quantity = i.Quantity,
                        Rate = i.Rate,
                        Amount = i.Amount,
                        DateOfCost = i.DateOfCost,
                        Memo = i.Memo,
                        ActualCtnsInThisOrder = shipOrderInDb.FBAPickDetails.Sum(x => x.ActualPlts),
                        ActualPltsInThisOrder = shipOrderInDb.FBAPickDetails.Sum(x => x.ActualQuantity),
                        DateOfClose = shipOrderInDb.CloseDate
                    });
                }

                if (shipOrderInDb.InvoiceStatus != "Closed")
                    shipOrderInDb.InvoiceStatus = "Exported";
                info.InvoiceReportDetails = invoiceReportList;
            }

            _context.SaveChanges();

            return info;
        }

        // 不包括未关闭的订单
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

            closeDate = closeDate.AddDays(1);

            var invoiceDetails = _context.InvoiceDetails
                .Include(x => x.FBAMasterOrder.Customer)
                .Include(x => x.FBAMasterOrder.FBAOrderDetails)
                .Include(x => x.FBAMasterOrder.FBAPallets)
                .Include(x => x.FBAShipOrder.FBAPickDetails)
                .Where(x => x.FBAMasterOrder.Customer.Id == customerId || x.FBAShipOrder.CustomerCode == customer.CustomerCode)
                .Where(x => x.FBAShipOrder == null ? x.FBAMasterOrder.CloseDate < closeDate && x.FBAMasterOrder.CloseDate >= startDate : x.FBAShipOrder.CloseDate >= startDate && x.FBAShipOrder.CloseDate < closeDate)
                //.Where(x => x.DateOfCost >= startDate && x.DateOfCost <= closeDate)
                .ToList();

            foreach(var i in invoiceDetails)
            {
                var newInvoiceDetail = new InvoiceReportDetail
                {
                    Cost = i.Cost,
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
                    newInvoiceDetail.GrandNumber = "N/A";
                    newInvoiceDetail.Reference = i.FBAShipOrder.ShipOrderNumber;
                    newInvoiceDetail.Destination = i.FBAShipOrder.Destination;
                    newInvoiceDetail.SubCustomer = i.FBAShipOrder.SubCustomer;
                    newInvoiceDetail.ActualCtnsInThisOrder = i.FBAShipOrder.FBAPickDetails.Sum(x => x.ActualQuantity);
                    newInvoiceDetail.ActualPltsInThisOrder = i.FBAShipOrder.FBAPickDetails.Sum(x => x.ActualPlts);
                    newInvoiceDetail.DateOfClose = i.FBAShipOrder.CloseDate;
                }
                else if (i.FBAShipOrder == null)
                {
                    newInvoiceDetail.GrandNumber = i.FBAMasterOrder.GrandNumber;
                    newInvoiceDetail.Reference = i.FBAMasterOrder.Container;
                    newInvoiceDetail.SubCustomer = i.FBAMasterOrder.SubCustomer;
                    newInvoiceDetail.Destination = " ";
                    newInvoiceDetail.ActualCtnsInThisOrder = i.FBAMasterOrder.FBAOrderDetails.Sum(x => x.ActualQuantity);
                    newInvoiceDetail.ActualPltsInThisOrder = i.FBAMasterOrder.FBAPallets.Sum(x => x.ActualPallets);
                    newInvoiceDetail.DateOfClose = i.FBAMasterOrder.CloseDate;
                }

                invoiceReportList.Add(newInvoiceDetail);
            }

            info.InvoiceReportDetails = invoiceReportList;

            return info;
        }

        // 包括未关闭的订单
        public FBAInvoiceInfo GetAllChargingReportFormDateRangeAndCustomerId(int customerId, DateTime startDate, DateTime closeDate)
        {
            var customer = _context.UpperVendors.Find(customerId);

            var invoiceReportList = new List<InvoiceReportDetail>();

            var info = new FBAInvoiceInfo
            {
                FromDate = startDate,
                ToDate = closeDate,
                CustomerCode = customer.CustomerCode
            };

            closeDate = closeDate.AddDays(1);

            var invoiceDetails = _context.InvoiceDetails
                .Include(x => x.FBAMasterOrder.Customer)
                .Include(x => x.FBAMasterOrder.FBAOrderDetails)
                .Include(x => x.FBAMasterOrder.FBAPallets)
                .Include(x => x.FBAShipOrder.FBAPickDetails)
                .Where(x => x.FBAMasterOrder.Customer.Id == customerId || x.FBAShipOrder.CustomerCode == customer.CustomerCode)
                .Where(x => x.FBAShipOrder == null ? (x.FBAMasterOrder.CloseDate < closeDate && x.FBAMasterOrder.CloseDate >= startDate) || x.FBAMasterOrder.CloseDate.Year == 1900 : (x.FBAShipOrder.CloseDate >= startDate && x.FBAShipOrder.CloseDate < closeDate) || x.FBAShipOrder.CloseDate.Year == 1900)
                //.Where(x => x.DateOfCost >= startDate && x.DateOfCost <= closeDate)
                .ToList();

            foreach (var i in invoiceDetails)
            {
                var newInvoiceDetail = new InvoiceReportDetail
                {
                    Cost = i.Cost,
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
                    newInvoiceDetail.GrandNumber = "N/A";
                    newInvoiceDetail.Reference = i.FBAShipOrder.ShipOrderNumber;
                    newInvoiceDetail.Destination = i.FBAShipOrder.Destination;
                    newInvoiceDetail.SubCustomer = i.FBAShipOrder.SubCustomer;
                    newInvoiceDetail.ActualCtnsInThisOrder = i.FBAShipOrder.FBAPickDetails.Sum(x => x.ActualQuantity);
                    newInvoiceDetail.ActualPltsInThisOrder = i.FBAShipOrder.FBAPickDetails.Sum(x => x.ActualPlts);
                    newInvoiceDetail.DateOfClose = i.FBAShipOrder.CloseDate;
                }
                else if (i.FBAShipOrder == null)
                {
                    newInvoiceDetail.GrandNumber = i.FBAMasterOrder.GrandNumber;
                    newInvoiceDetail.Reference = i.FBAMasterOrder.Container;
                    newInvoiceDetail.SubCustomer = i.FBAMasterOrder.SubCustomer;
                    newInvoiceDetail.Destination = " ";
                    newInvoiceDetail.ActualCtnsInThisOrder = i.FBAMasterOrder.FBAOrderDetails.Sum(x => x.ActualQuantity);
                    newInvoiceDetail.ActualPltsInThisOrder = i.FBAMasterOrder.FBAPallets.Sum(x => x.ActualPallets);
                    newInvoiceDetail.DateOfClose = i.FBAMasterOrder.CloseDate;
                }

                invoiceReportList.Add(newInvoiceDetail);
            }

            info.InvoiceReportDetails = invoiceReportList;

            return info;
        }

        public FBAInvoiceInfo GetAllFBACustomerChargingReportFromDate(DateTime startDate, DateTime closeDate)
        {
            var info = new FBAInvoiceInfo { CustomerCode = "ALL FBA CUSTOMER", FromDate = startDate, ToDate = closeDate };
            var invoiceReportList = new List<InvoiceReportDetail>();

            closeDate = closeDate.AddDays(1);

            var invoiceDetails = _context.InvoiceDetails
                .Include(x => x.FBAMasterOrder.Customer)
                .Include(x => x.FBAMasterOrder.FBAOrderDetails)
                .Include(x => x.FBAMasterOrder.FBAPallets)
                .Include(x => x.FBAShipOrder.FBAPickDetails)
                .Where(x => x.FBAMasterOrder.Customer.DepartmentCode == "FBA" || x.FBAShipOrder.Id > 0)
                .Where(x => x.FBAShipOrder == null ? x.FBAMasterOrder.CloseDate < closeDate && x.FBAMasterOrder.CloseDate >= startDate : x.FBAShipOrder.CloseDate >= startDate && x.FBAShipOrder.CloseDate < closeDate)
                .ToList();

            foreach (var i in invoiceDetails)
            {
                var newInvoiceDetail = new InvoiceReportDetail
                {
                    Cost = i.Cost,
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
                    newInvoiceDetail.CustomerCode = i.FBAShipOrder.CustomerCode;
                    newInvoiceDetail.GrandNumber = "N/A";
                    newInvoiceDetail.Reference = i.FBAShipOrder.ShipOrderNumber;
                    newInvoiceDetail.SubCustomer = i.FBAShipOrder.SubCustomer;
                    newInvoiceDetail.Destination = i.FBAShipOrder.Destination;
                    newInvoiceDetail.ActualCtnsInThisOrder = i.FBAShipOrder.FBAPickDetails.Sum(x => x.ActualQuantity);
                    newInvoiceDetail.ActualPltsInThisOrder = i.FBAShipOrder.FBAPickDetails.Sum(x => x.ActualPlts);
                    newInvoiceDetail.DateOfClose = i.FBAShipOrder.CloseDate;
                }
                else if (i.FBAShipOrder == null)
                {
                    newInvoiceDetail.CustomerCode = i.FBAMasterOrder.Customer.CustomerCode;
                    newInvoiceDetail.GrandNumber = i.FBAMasterOrder.GrandNumber;
                    newInvoiceDetail.SubCustomer = i.FBAMasterOrder.SubCustomer;
                    newInvoiceDetail.Reference = i.FBAMasterOrder.Container;
                    newInvoiceDetail.Destination = " ";
                    newInvoiceDetail.ActualCtnsInThisOrder = i.FBAMasterOrder.FBAOrderDetails.Sum(x => x.ActualQuantity);
                    newInvoiceDetail.ActualPltsInThisOrder = i.FBAMasterOrder.FBAPallets.Sum(x => x.ActualPallets);
                    newInvoiceDetail.DateOfClose = i.FBAMasterOrder.CloseDate;
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

        public DateTime FromDate { get; set; }

        public DateTime ToDate { get; set; }

        public List<InvoiceReportDetail> InvoiceReportDetails { get; set; }
    }
    
    public class InvoiceReportDetail
    {
        public string CustomerCode { get; set; }

        public string SubCustomer { get; set; }

        public double Cost { get; set; }

        public string Reference { get; set; }

        public string Activity { get; set; }

        public string GrandNumber { get; set; }

        public string InvoiceType { get; set; }

        public int ActualCtnsInThisOrder { get; set; }

        public int ActualPltsInThisOrder { get; set; }

        public string ChargingType { get; set; }

        public string Unit { get; set; }

        public double Quantity { get; set; }

        public DateTime DateOfClose { get; set; }

        public double Rate { get; set; }

        public double Amount { get; set; }

        public int Pallets { get; set; }

        public string Destination { get; set; }

        public DateTime DateOfCost { get; set; }

        public string Memo { get; set; }
    }
}