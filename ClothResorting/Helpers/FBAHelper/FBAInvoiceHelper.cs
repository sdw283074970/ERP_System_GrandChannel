﻿using ClothResorting.Models;
using ClothResorting.Models.StaticClass;
using Microsoft.Office.Interop.Excel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.Entity;
using ClothResorting.Models.FBAModels.StaticModels;
using ClothResorting.Models.FBAModels;
using ClothResorting.Controllers.Api.USPrime;
using Aspose.Cells;

namespace ClothResorting.Helpers.FBAHelper
{
    public class FBAInvoiceHelper
    {
        private ApplicationDbContext _context;
        private string _path = "";
        private _Application _excel;
        private Microsoft.Office.Interop.Excel.Workbook _wb;
        private Microsoft.Office.Interop.Excel.Worksheet _ws;
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

        //public FBAInvoiceHelper(string templatePath, bool isPrime)
        //{
        //    asposeWb = new Aspose.Cells.Workbook(templatePath);
        //}

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
                _ws.Cells[4, 6] = info.ToDate == null ? "" : info.ToDate.AddDays(-1).ToString("yyyy-MM-dd").Substring(0, 10);
                _ws.Cells[4, 8] = DateTime.Now.ToString("yyyy-MM-dd");
            }

            //制作第一个Summary表
            _ws = _wb.Worksheets[1];

            _ws.Cells[8, 19] = "Warehouse Loc";
            _ws.Cells[8, 20] = "Carrier";

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
                _ws.Cells[startRow, 8] = Math.Round(i.Quantity, 2);
                _ws.Cells[startRow, 9] = Math.Round(i.Rate, 2);
                _ws.Cells[startRow, 10] = Math.Round(i.Discount, 2);
                _ws.Cells[startRow, 11] = Math.Round(i.OriginalAmount, 2);
                _ws.Cells[startRow, 12] = Math.Round(i.Amount, 2);
                _ws.Cells[startRow, 13] = Math.Round(i.Cost, 2);
                _ws.Cells[startRow, 14] = i.DateOfCost.ToString("yyyy-MM-dd");
                _ws.Cells[startRow, 15] = i.Memo;
                _ws.Cells[startRow, 16] = i.IsConfirmedCost ? "YES" : "NO";
                _ws.Cells[startRow, 17] = i.IsPayed ? "YES" : "NO";
                _ws.Cells[startRow, 18] = i.IsCollected ? "YES" : "NO";
                _ws.Cells[startRow, 19] = i.WarehouseLocation;
                _ws.Cells[startRow, 20] = i.Carrier;

                startRow += 1;
            }

            _ws.Cells[startRow, 9] = "Total";
            _ws.Cells[startRow, 11] = Math.Round(info.InvoiceReportDetails.Sum(x => x.OriginalAmount), 2);
            _ws.Cells[startRow, 12] = Math.Round(info.InvoiceReportDetails.Sum(x => x.Amount), 2);
            _ws.Cells[startRow, 13] = Math.Round(info.InvoiceReportDetails.Sum(x => x.Cost), 2);

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
            _ws.Cells[startRow, columnIndex + 3] = "Warehouse Loc";
            _ws.Cells[startRow, columnIndex + 4] = "Carrier";

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
                _ws.Cells[startRow, columnIndex + 1] = Math.Round(r.Sum(x => x.Amount), 2);
                _ws.Cells[startRow, columnIndex + 2] = Math.Round(r.Sum(x => x.Cost), 2);
                _ws.Cells[startRow, columnIndex + 3] = r.First().WarehouseLocation;
                _ws.Cells[startRow, columnIndex + 4] = r.First().Carrier;

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
            _ws.Cells[startRow, columnIndex + 1] = Math.Round(info.InvoiceReportDetails.Sum(x => x.Amount), 2);
            _ws.Cells[startRow, columnIndex + 2] = Math.Round(info.InvoiceReportDetails.Sum(x => x.Cost), 2);

            //制作第三个收费细节表
            _ws = _wb.Worksheets[3];

            startRow = 6;

            var shipOrderList = _context.FBAShipOrders
                .Include(x => x.FBAPickDetails)
                .Where(x => x.CustomerCode == info.CustomerCode
                    && x.ShipDate >= info.FromDate
                    && x.ShipDate < info.ToDate)
                .ToList();

            foreach(var s in shipOrderList)
            {
                _ws.Cells[startRow, 1] = "Reference";
                _ws.Cells[startRow, 2] = s.ShipOrderNumber;
                _ws.Cells[startRow, 3] = "Outbound Date";
                _ws.Cells[startRow, 4] = s.ShipDate.ToString("yyyy-MM-dd");
                _ws.Cells[startRow, 5] = "Warehouse Location";
                _ws.Cells[startRow, 6] = s.WarehouseLocation;
                _ws.Cells[startRow, 7] = "Carrier";
                _ws.Cells[startRow, 8] = s.Carrier;
                _ws.Cells[startRow, 9] = "Sub-code";
                _ws.Cells[startRow, 10] = s.SubCustomer;

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

            var fullPath = @"E:\ChargingReport\FBA-" + info.CustomerCode + "-ChargingReport-" + DateTime.Now.ToString("yyyyMMddhhmmssffff") + ".xls";
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
            _ws.Cells[4, 6] = info.ToDate == null ? "" : info.ToDate.AddDays(-1).ToString("yyyy-MM-dd").Substring(0, 10);
            _ws.Cells[4, 8] = DateTime.Now.ToString("yyyy-MM-dd");

            var groupByCustomer = info.InvoiceReportDetails.GroupBy(x => x.CustomerCode);
            var startRow = 8;

            foreach (var g in groupByCustomer)
            {
                _ws.Cells[startRow - 1, 1] = g.First().CustomerCode;
                _ws.Cells[startRow, 1] = "Order Type";
                _ws.Cells[startRow, 2] = "Reference #";
                _ws.Cells[startRow, 3] = "Grand #";
                _ws.Cells[startRow, 4] = "Subcustomer";
                _ws.Cells[startRow, 5] = "Activity";
                _ws.Cells[startRow, 6] = "Charging Type";
                _ws.Cells[startRow, 7] = "UOM";
                _ws.Cells[startRow, 8] = "Quantity";
                _ws.Cells[startRow, 9] = "Rate";
                _ws.Cells[startRow, 10] = "Discount";
                _ws.Cells[startRow, 11] = "Org Amount";
                _ws.Cells[startRow, 12] = "Amout";
                _ws.Cells[startRow, 13] = "Cost";
                _ws.Cells[startRow, 14] = "Order Cost Date";
                _ws.Cells[startRow, 15] = "Memo";
                _ws.Cells[startRow, 16] = "Cost Confirm";
                _ws.Cells[startRow, 17] = "Payed";
                _ws.Cells[startRow, 18] = "Collected";
                _ws.Cells[startRow, 19] = "Warehouse Loc";
                _ws.Cells[startRow, 20] = "Carrier";

                startRow += 1;

                foreach (var i in g)
                {
                    //_ws.Cells[startRow, 1] = i.InvoiceType;
                    //_ws.Cells[startRow, 2] = i.Reference;
                    //_ws.Cells[startRow, 3] = i.GrandNumber;
                    //_ws.Cells[startRow, 4] = i.Activity;
                    //_ws.Cells[startRow, 5] = i.ChargingType;
                    //_ws.Cells[startRow, 6] = i.Unit;
                    //_ws.Cells[startRow, 7] = i.Quantity;
                    //_ws.Cells[startRow, 8] = i.Rate;
                    //_ws.Cells[startRow, 9] = i.Amount;
                    //_ws.Cells[startRow, 10] = i.DateOfCost.ToString("yyyy-MM-dd");
                    //_ws.Cells[startRow, 11] = i.Memo;
                    //_ws.Cells[startRow, 12] = i.Cost;
                    //_ws.Cells[startRow, 16] = i.IsConfirmedCost ? "YES" : "NO";
                    //_ws.Cells[startRow, 17] = i.IsPayed ? "YES" : "NO";
                    //_ws.Cells[startRow, 18] = i.IsCollected ? "YES" : "NO";
                    _ws.Cells[startRow, 1] = i.InvoiceType;
                    _ws.Cells[startRow, 2] = i.Reference;
                    _ws.Cells[startRow, 3] = i.GrandNumber;
                    _ws.Cells[startRow, 4] = i.SubCustomer ?? "N/A";
                    _ws.Cells[startRow, 5] = i.Activity;
                    _ws.Cells[startRow, 6] = i.ChargingType;
                    _ws.Cells[startRow, 7] = i.Unit;
                    _ws.Cells[startRow, 8] = Math.Round(i.Quantity, 2);
                    _ws.Cells[startRow, 9] = Math.Round(i.Rate, 2);
                    _ws.Cells[startRow, 10] = Math.Round(i.Discount, 2);
                    _ws.Cells[startRow, 11] = Math.Round(i.OriginalAmount, 2);
                    _ws.Cells[startRow, 12] = Math.Round(i.Amount, 2);
                    _ws.Cells[startRow, 13] = Math.Round(i.Cost, 2);
                    //_ws.Cells[startRow, 14] = i.DateOfCost.ToString("yyyy-MM-dd");
                    _ws.Cells[startRow, 14] = i.DateOfClose.ToString("yyyy-MM-dd");
                    _ws.Cells[startRow, 15] = i.Memo;
                    _ws.Cells[startRow, 16] = i.IsConfirmedCost ? "YES" : "NO";
                    _ws.Cells[startRow, 17] = i.IsPayed ? "YES" : "NO";
                    _ws.Cells[startRow, 18] = i.IsCollected ? "YES" : "NO";
                    _ws.Cells[startRow, 19] = i.WarehouseLocation;
                    _ws.Cells[startRow, 20] = i.Carrier;
                    startRow += 1;
                }
                _ws.Cells[startRow, 8] = "Total";
                _ws.Cells[startRow, 9] = g.Sum(x => x.Amount);
                _ws.Cells[startRow, 12] = g.Sum(x => x.Cost);

                startRow += 3;
            }

            var fullPath = @"E:\ChargingReport\FBA-" + info.CustomerCode + "-ChargingReport-" + DateTime.Now.ToString("yyyyMMddhhmmssffff") + ".xls";
            _wb.SaveAs(fullPath, Type.Missing, "", "", Type.Missing, Type.Missing, XlSaveAsAccessMode.xlNoChange, 1, false, Type.Missing, Type.Missing, Type.Missing);

            _excel.Quit();

            //var killer = new ExcelKiller();

            //killer.Dispose();

            return fullPath;
        }

        public string GenerateSingleDN(USPrimeOrderDto order)
        {
            _ws = _wb.Worksheets[1];

            _ws.Cells[2, 11] = order.hblNumber;
            _ws.Cells[10, 3] = order.customerName;
            _ws.Cells[11, 3] = order.address_1;
            _ws.Cells[12, 3] = order.address_2;
            _ws.Cells[10, 11] = order.dnDate.ToString("yyyy-MM-dd");
            _ws.Cells[13, 11] = order.by;

            //_ws.Cells[15, 2].PutValue("HB/L# " + order.hblNumber.Substring(6));
            //_ws.Cells[16, 2].PutValue("MB/L# " + order.mblNumber);

            //_ws.Cells[18, 3].PutValue("Trucking Fee");
            //_ws.Cells[18, 9].PutValue(1);
            //_ws.Cells[18, 10].PutValue(order.truckingFee);

            //_ws.Cells[19, 3].PutValue("Handling Fee");
            //_ws.Cells[19, 9].PutValue(1);
            //_ws.Cells[19, 10].PutValue(order.handlingFee);

            //if (order.profitShare != 0)
            //{
            //    _ws.Cells[20, 3].PutValue("Profit Share");
            //    _ws.Cells[20, 9].PutValue(1);
            //    _ws.Cells[20, 10].PutValue(order.profitShare);
            //}

            //_ws.Cells[38, 2].PutValue(order.OriginNote);

            _ws.Cells[15, 2] = "HB/L# " + order.hblNumber.Substring(6);
            _ws.Cells[16, 2] = "MB/L# " + order.mblNumber;

            if (order.truckingFee != 0)
            {
                _ws.Cells[18, 3] = "Trucking Fee";
                _ws.Cells[18, 9] = 1;
                _ws.Cells[18, 10] = order.truckingFee;
            }

            if (order.handlingFee != 0)
            {
                _ws.Cells[19, 3] = "Handling Fee";
                _ws.Cells[19, 9] = 1;
                _ws.Cells[19, 10] = order.handlingFee;
            }

            if (order.profitShare != 0)
            {
                _ws.Cells[20, 3] = "Profit Share";
                _ws.Cells[20, 9] = 1;
                _ws.Cells[20, 10] = order.profitShare;
            }

            _ws.Cells[38, 2] = order.OriginNote;

            _excel.DisplayAlerts = false;

            var xlsxPath = @"E:\usprime\DN\DN-" + order.customerName + "-" + order.dnDate.ToString("yyyyMMdd") + ".xlsx";
            _wb.SaveAs(xlsxPath, Type.Missing, "", "", Type.Missing, Type.Missing, XlSaveAsAccessMode.xlNoChange, 1, false, Type.Missing, Type.Missing, Type.Missing);

            var pdfPath = @"E:\usprime\DN\DN-" + order.customerName + "-" + order.dnDate.ToString("yyyyMMdd") + ".pdf";
            _wb.ExportAsFixedFormat(XlFixedFormatType.xlTypePDF, pdfPath);

            _excel.Quit();

            return pdfPath;
        }

        public static string GenerateSOA(string templatePath, SOA soa)
        {
            var excel = new Application();
            var wb = excel.Workbooks.Open(templatePath);
            var ws = wb.Worksheets[1];

            ws.Cells[10, 3] = soa.customerName;
            ws.Cells[11, 3] = soa.address_1;
            ws.Cells[12, 3] = soa.address_2;
            ws.Cells[10, 10] = soa.fromDate.ToString("yyyy-MM-dd");
            ws.Cells[11, 10] = soa.toDate.ToString("yyyy-MM-dd");
            ws.Cells[12, 10] = soa.reportDate.ToString("yyyy-MM-dd");
            ws.Cells[13, 10] = soa.by;

            var index = 18;

            foreach(var e in soa.entries)
            {
                ws.Cells[index, 2] = e.hblNumber;
                ws.Cells[index, 4] = e.mblNumber;
                ws.Cells[index, 6] = e.etd;
                ws.Cells[index, 7] = e.releasedDate;
                ws.Cells[index, 9] = e.balanceToOrigin;
                //_ws.Cells[index, 10] = e.note;

                index++;
            }

            //_ws.Cells[index, 6] = "ACCT";
            ws.Cells[index, 7] = "BALANCE";
            ws.Cells[index, 9] = soa.entries.Sum(x => x.balanceToOrigin);
            excel.DisplayAlerts = false;

            var xlsxPath = @"E:\usprime\SOA\SOA-" + soa.customerName + "-" + soa.fromDate.ToString("yyyyMMdd") + "-" + soa.toDate.ToString("yyyyMMdd") + ".xlsx";
            wb.SaveAs(xlsxPath, Type.Missing, "", "", Type.Missing, Type.Missing, XlSaveAsAccessMode.xlNoChange, 1, false, Type.Missing, Type.Missing, Type.Missing);

            excel.Quit();
            GC.Collect();
            //var wb = new Aspose.Cells.Workbook(xlsxPath);
            //var pdfPath = @"E:\usprime\SOA\SOA-" + soa.customerName + "-" + soa.fromDate.ToString("yyyyMMdd") + "-" + soa.toDate.ToString("yyyyMMdd") + ".pdf";
            //wb.Save(pdfPath, SaveFormat.Pdf);

            //_wb.ExportAsFixedFormat(XlFixedFormatType.xlTypePDF, pdfPath);
            return xlsxPath;
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
                        Carrier = i.FBAMasterOrder == null ? i.FBAShipOrder.Carrier : i.FBAMasterOrder.Carrier,
                        WarehouseLocation = i.FBAMasterOrder == null ? i.FBAShipOrder.WarehouseLocation : i.FBAMasterOrder.WarehouseLocation,
                        Rate = i.Rate,
                        Amount = i.Amount,
                        DateOfCost = i.DateOfCost,
                        Memo = i.Memo,
                        Discount = i.Discount,
                        OriginalAmount = Math.Round(i.Amount / (double)i.Discount, 2),
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
                        Carrier = i.FBAMasterOrder == null ? i.FBAShipOrder.Carrier : i.FBAMasterOrder.Carrier,
                        WarehouseLocation = i.FBAMasterOrder == null ? i.FBAShipOrder.WarehouseLocation : i.FBAMasterOrder.WarehouseLocation,
                        Amount = i.Amount,
                        DateOfCost = i.DateOfCost,
                        Memo = i.Memo,
                        Discount = i.Discount,
                        OriginalAmount = Math.Round(i.Amount / (double)i.Discount, 2),
                        ActualCtnsInThisOrder = shipOrderInDb.FBAPickDetails.Sum(x => x.ActualPlts),
                        ActualPltsInThisOrder = shipOrderInDb.FBAPickDetails.Sum(x => x.ActualQuantity),
                        DateOfClose = shipOrderInDb.CloseDate,
                        IsCollected = i.CollectionStatus,
                        IsConfirmedCost = i.CostConfirm,
                        IsPayed = i.PaymentStatus
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
        public FBAInvoiceInfo GetClosedChargingReportFormDateRangeAndCustomerId(int customerId, DateTime startDate, DateTime closeDate, string[] warehouseLocations)
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
                .Include(x => x.FBAMasterOrder.FBAOrderDetails)
                .Include(x => x.FBAMasterOrder.FBAPallets)
                .Include(x => x.FBAShipOrder.FBAPickDetails)
                .Where(x => x.FBAMasterOrder.Customer.Id == customerId || x.FBAShipOrder.CustomerCode == customer.CustomerCode)
                .Where(x => x.FBAShipOrder == null ? (x.FBAMasterOrder.CloseDate < closeDate && x.FBAMasterOrder.CloseDate >= startDate && x.FBAMasterOrder.InvoiceStatus == FBAStatus.Closed) : (x.FBAShipOrder.CloseDate >= startDate && x.FBAShipOrder.CloseDate < closeDate && x.FBAShipOrder.InvoiceStatus == FBAStatus.Closed))
                .Where(x => x.FBAShipOrder == null ? (warehouseLocations.Contains(x.FBAMasterOrder.WarehouseLocation)) : (warehouseLocations.Contains(x.FBAShipOrder.WarehouseLocation)))
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
                    Carrier = i.FBAMasterOrder == null ? i.FBAShipOrder.Carrier : i.FBAMasterOrder.Carrier,
                    WarehouseLocation = i.FBAMasterOrder == null ? i.FBAShipOrder.WarehouseLocation : i.FBAMasterOrder.WarehouseLocation,
                    IsConfirmedCost = i.CostConfirm,
                    IsPayed = i.PaymentStatus,
                    IsCollected = i.CollectionStatus,
                    Amount = i.Amount,
                    DateOfCost = i.DateOfCost,
                    Memo = i.Memo,
                    Discount = i.Discount,
                    OriginalAmount = Math.Round(i.Amount / (double)i.Discount, 2)
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
        public FBAInvoiceInfo GetAllChargingReportFormDateRangeAndCustomerId(int customerId, DateTime startDate, DateTime closeDate, string[] warehouseLocations)
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
                .Include(x => x.FBAMasterOrder.FBAOrderDetails)
                .Include(x => x.FBAMasterOrder.FBAPallets)
                .Include(x => x.FBAShipOrder.FBAPickDetails)
                .Where(x => x.FBAMasterOrder.Customer.Id == customerId || x.FBAShipOrder.CustomerCode == customer.CustomerCode)
                .Where(x => x.FBAShipOrder == null ? (x.FBAMasterOrder.CloseDate < closeDate && x.FBAMasterOrder.CloseDate >= startDate) || x.FBAMasterOrder.CloseDate.Year == 1900 : (x.FBAShipOrder.CloseDate >= startDate && x.FBAShipOrder.CloseDate < closeDate) || x.FBAShipOrder.CloseDate.Year == 1900)
                .Where(x => x.FBAShipOrder == null ? (warehouseLocations.Contains(x.FBAMasterOrder.WarehouseLocation)) : (warehouseLocations.Contains(x.FBAShipOrder.WarehouseLocation)))
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
                    Carrier = i.FBAMasterOrder == null ? i.FBAShipOrder.Carrier : i.FBAMasterOrder.Carrier,
                    WarehouseLocation = i.FBAMasterOrder == null ? i.FBAShipOrder.WarehouseLocation : i.FBAMasterOrder.WarehouseLocation,
                    IsConfirmedCost = i.CostConfirm,
                    IsPayed = i.PaymentStatus,
                    IsCollected = i.CollectionStatus,
                    Amount = i.Amount,
                    DateOfCost = i.DateOfCost,
                    Memo = i.Memo,
                    Discount = i.Discount,
                    OriginalAmount = Math.Round(i.Amount / (double)i.Discount, 2)
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

        public FBAInvoiceInfo GetAllFBACustomerChargingReportFromDate(DateTime startDate, DateTime closeDate, string[] warehouseLocations)
        {
            var info = new FBAInvoiceInfo { CustomerCode = "ALL FBA CUSTOMER", FromDate = startDate, ToDate = closeDate };
            var invoiceReportList = new List<InvoiceReportDetail>();

            var invoiceDetails = _context.InvoiceDetails
                .Include(x => x.FBAMasterOrder.Customer)
                .Include(x => x.FBAMasterOrder.FBAOrderDetails)
                .Include(x => x.FBAMasterOrder.FBAPallets)
                .Include(x => x.FBAShipOrder.FBAPickDetails)
                .Where(x => x.FBAMasterOrder.Customer.DepartmentCode == "FBA" || x.FBAShipOrder.Id > 0)
                //.Where(x => x.FBAShipOrder == null ? (x.FBAMasterOrder.CloseDate < closeDate && x.FBAMasterOrder.CloseDate >= startDate && x.FBAMasterOrder.InvoiceStatus == FBAStatus.Closed) : (x.FBAShipOrder.CloseDate >= startDate && x.FBAShipOrder.CloseDate < closeDate && x.FBAShipOrder.InvoiceStatus == FBAStatus.Closed))
                .Where(x => x.FBAShipOrder == null ? (x.FBAMasterOrder.CloseDate < closeDate && x.FBAMasterOrder.CloseDate >= startDate) : (x.FBAShipOrder.CloseDate >= startDate && x.FBAShipOrder.CloseDate < closeDate))
                .Where(x => x.FBAShipOrder == null ? (warehouseLocations.Contains(x.FBAMasterOrder.WarehouseLocation)) : (warehouseLocations.Contains(x.FBAShipOrder.WarehouseLocation)))
                .Where(x => x.FBAShipOrder.Carrier.Contains("YAO") || x.FBAShipOrder.Carrier.Contains("yao") || x.FBAShipOrder.Carrier.Contains("Yao") || x.FBAMasterOrder.Carrier.Contains("YAO") || x.FBAMasterOrder.Carrier.Contains("yao") || x.FBAMasterOrder.Carrier.Contains("Yao"))
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
                    IsConfirmedCost = i.CostConfirm,
                    Carrier = i.FBAMasterOrder == null ? i.FBAShipOrder.Carrier : i.FBAMasterOrder.Carrier,
                    WarehouseLocation = i.FBAMasterOrder == null ? i.FBAShipOrder.WarehouseLocation : i.FBAMasterOrder.WarehouseLocation,
                    IsPayed = i.PaymentStatus,
                    IsCollected = i.CollectionStatus,
                    Amount = i.Amount,
                    DateOfCost = i.DateOfCost,
                    Memo = i.Memo,
                    Discount = i.Discount,
                    OriginalAmount = Math.Round(i.Amount / (double)i.Discount, 2)
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

        public IList<ContainerFeeSummary> GetContainerFeeSummary(string customerCode, DateTime startDate, DateTime endDate)
        {
            var results = new List<ContainerFeeSummary>();

            var masterOrderList = _context.FBAMasterOrders
                .Include(x => x.FBAOrderDetails)
                .Include(x => x.InvoiceDetails)
                .Include(x => x.FBAPallets)
                .Where(x => x.CustomerCode == customerCode)
                .Where(x => x.InboundDate >= startDate && x.InboundDate < endDate)
                .ToList();

            var containerList = masterOrderList.Select(x => x.Container).ToList();

            //var shipOrderList = _context.FBAShipOrders
            //    .Include(x => x.FBAPickDetails)
            //    .Include(x => x.InvoiceDetails)
            //    .Where(x => x.CustomerCode == customerCode)
            //    .Where(x => x.ReleasedDate >= startDate && x.ReleasedDate < endDate)
            //    .ToList();

            var pickDetailGroupByContainer = _context.FBAPickDetails
                .Include(x => x.FBAShipOrder)
                .Include(x => x.FBAShipOrder.InvoiceDetails)
                .Include(x => x.FBAShipOrder.FBAPickDetails)
                .Where(x => x.FBAShipOrder.CustomerCode == customerCode)
                .Where(x => containerList.Contains(x.Container))
                .ToList()
                .GroupBy(x => x.Container)
                .ToList();

            foreach(var m in masterOrderList)
            {
                results.Add(new ContainerFeeSummary {
                    CustomerCode = customerCode,
                    Container = m.Container,
                    SubCustomer = m.SubCustomer,
                    OriginalCtns = m.FBAOrderDetails.Sum(x => x.Quantity),
                    ActualCtns = m.FBAOrderDetails.Sum(x => x.ActualQuantity),
                    OriginalPlts = m.OriginalPlts,
                    ActualPlts = m.FBAPallets.Sum(x => x.ActualPallets),
                    InvoiceStatus = m.InvoiceStatus,
                    CloseDate = m.CloseDate,
                    InboundDate = m.InboundDate,
                    TotalAmount = (float)Math.Round(m.InvoiceDetails.Sum(x => x.Amount), 2),
                    ChargingCounts = m.InvoiceDetails.Count
                });
            }

            // 找到所有与当前container相关的出库单invoiceDetails
            var shipOrderInvoiceDetails = new List<InvoiceDetail>();
            var filteredPickDetails = new List<FBAPickDetail>();

            foreach (var containerGroup in pickDetailGroupByContainer)
            {
                // 统计按照货柜号分组的拣货记录，按照shiporder再次分组
                var pickDetailGroupByShipOrders = containerGroup.GroupBy(x => x.FBAShipOrder).ToList();

                foreach (var shipOrderGroup in pickDetailGroupByShipOrders)
                {
                    //filteredPickDetails.AddRange(shipOrderGroup);
                    var pickDetail = shipOrderGroup.First();
                    pickDetail.ActualQuantity = shipOrderGroup.Sum(x => x.ActualQuantity);
                    pickDetail.PickableCtns = pickDetail.FBAShipOrder.FBAPickDetails.Sum(x => x.PickableCtns);
                    //pickDetail.GrandNumber = $"{pickDetail.ActualQuantity} / {pickDetail.PickableCtns}";
                    filteredPickDetails.Add(pickDetail);
                }
            }
            
            foreach (var f in filteredPickDetails)
            {
                var shipOrder = f.FBAShipOrder;

                var currentQuantity = f.ActualQuantity;
                var totalQuanity = f.PickableCtns;
                var percent = (float)Math.Round((double)currentQuantity / totalQuanity, 2);

                foreach (var i in shipOrder.InvoiceDetails)
                {
                    shipOrderInvoiceDetails.Add(new InvoiceDetail {
                        Activity = i.Activity,
                        ChargingType = i.ChargingType,
                        Memo = i.Memo,
                        //Memo = f.GrandNumber,
                        Unit = i.Unit,
                        Rate = i.Rate,
                        Discount = i.Discount,
                        Quantity = Math.Round(i.Quantity * percent, 2),
                        OriginalAmount = Math.Round(i.Quantity * i.Rate * percent, 2),
                        Amount = Math.Round(i.Amount * percent, 2),
                        Operator = f.Container, // 借位记录占比
                        Cost = Math.Round(percent, 2),   // 借位记录占比
                        DateOfCost = i.DateOfCost,
                        FBAShipOrder = i.FBAShipOrder
                    });
                }
            }

            foreach (var r in results)
            {
                var invoiceDetailList = new List<ContainerFeeDetail>();

                // 添加主单收费条目，全部添加
                var masterOrder = masterOrderList.SingleOrDefault(x => x.Container == r.Container);

                foreach(var i in masterOrder.InvoiceDetails)
                {
                    invoiceDetailList.Add(new ContainerFeeDetail {
                        Activity = i.Activity,
                        OrderReference = masterOrder.Container,
                        OrderType = FBAOrderType.Inbound,
                        ChargingType = i.ChargingType,
                        UOM = i.Unit,
                        Quantity = (float)Math.Round(i.Quantity, 2),
                        Rate = (float)Math.Round(i.Rate, 2),
                        OriginalAmount = (float)Math.Round(i.OriginalAmount, 2),
                        DiscountRate = (float)Math.Round(i.Discount, 2),
                        FinalAmount = (float)Math.Round(i.Amount, 2),
                        Memo = i.Memo,
                        DateOfCost = i.DateOfCost,
                        Percent = "100%"
                    });
                }

                var currentContainerInvoiceDetails = shipOrderInvoiceDetails.Where(x => x.Operator == r.Container);

                foreach (var i in currentContainerInvoiceDetails)
                {
                    invoiceDetailList.Add(new ContainerFeeDetail {
                        Activity = i.Activity,
                        OrderReference = i.FBAShipOrder.ShipOrderNumber,
                        OrderType = FBAOrderType.Outbound,
                        ChargingType = i.ChargingType,
                        UOM = i.Unit,
                        Quantity = (float)Math.Round(i.Quantity, 2),
                        Rate = (float)Math.Round(i.Rate, 2),
                        OriginalAmount = (float)Math.Round(i.OriginalAmount, 2),
                        DiscountRate = (float)Math.Round(i.Discount, 2),
                        FinalAmount = (float)Math.Round(i.Amount, 2),
                        Memo = i.Memo,
                        DateOfCost = i.DateOfCost,
                        Percent = (i.Cost * 100).ToString() + "%"
                    });
                }

                r.TotalAmount += (float)currentContainerInvoiceDetails.Sum(x => x.Amount);
                r.ChargingCounts += currentContainerInvoiceDetails.Count();
                r.ContianerFeeDetails = invoiceDetailList;
            }

            return results;
        }

        public string GenerateContainerReport(string customerCode, DateTime startDate, DateTime endDate)
        {
            var list = GetContainerFeeSummary(customerCode, startDate, endDate);

            _ws = _wb.Worksheets[1];

            var index = 3;

            foreach (var c in list)
            {
                foreach (var d in c.ContianerFeeDetails)
                {
                    if (d.Percent != "100%")
                        continue;

                    _ws.Cells[index, 1] = d.OrderReference;
                    _ws.Cells[index, 2] = d.OrderType;
                    _ws.Cells[index, 3] = d.Activity;
                    _ws.Cells[index, 4] = d.ChargingType;
                    _ws.Cells[index, 5] = d.UOM;
                    _ws.Cells[index, 6] = d.Quantity;
                    _ws.Cells[index, 7] = d.Rate;
                    _ws.Cells[index, 8] = d.OriginalAmount;
                    _ws.Cells[index, 9] = d.DiscountRate;
                    _ws.Cells[index, 10] = d.FinalAmount;

                    index++;
                }
            }

            var fullPath = @"E:\ChargingReport\FBA-" + customerCode + "-ContainerReport-" + DateTime.Now.ToString("yyyyMMddhhmmssffff") + ".xlsx";

            _wb.SaveAs(fullPath, Type.Missing, "", "", Type.Missing, Type.Missing, XlSaveAsAccessMode.xlNoChange, 1, false, Type.Missing, Type.Missing, Type.Missing);

            _excel.Quit();

            var killer = new ExcelKiller();

            killer.Dispose();

            return fullPath;
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

        public bool IsConfirmedCost { get; set; }

        public bool IsPayed { get; set; }

        public bool IsCollected { get; set; }

        public double Discount { get; set; }

        public double OriginalAmount { get; set; }

        public double Amount { get; set; }

        public int Pallets { get; set; }

        public string Destination { get; set; }

        public DateTime DateOfCost { get; set; }

        public string Memo { get; set; }

        public string Carrier { get; set; }

        public string WarehouseLocation { get; set; }
    }

    public class ContainerFeeSummary
    {
        public string Container { get; set; }

        public DateTime InboundDate { get; set; }

        public string CustomerCode { get; set; }

        public string SubCustomer { get; set; }

        public int OriginalCtns { get; set; }

        public int ActualCtns { get; set; }

        public int OriginalPlts { get; set; }

        public int ActualPlts { get; set; }

        public float TotalAmount { get; set; }

        public string InvoiceStatus { get; set; }

        public DateTime CloseDate { get; set; }

        public int ChargingCounts { get; set; }

        public IList<ContainerFeeDetail> ContianerFeeDetails { get; set; }
    }

    public class ContainerFeeDetail
    {
        public string Activity { get; set; }

        public string Percent { get; set; }

        public string OrderReference { get; set; }

        public string OrderType { get; set; }

        public string UOM { get; set; }

        public string ChargingType { get; set; }

        public float Quantity { get; set; }

        public float Rate { get; set; }

        public float OriginalAmount { get; set; }

        public float DiscountRate { get; set; }

        public float FinalAmount { get; set; }

        public string Memo { get; set; }

        public DateTime DateOfCost { get; set; }
    }
}