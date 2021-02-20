using ClothResorting.Models;
using Microsoft.Office.Interop.Excel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Entity;
using ClothResorting.Models.StaticClass;
using ClothResorting.Models.FBAModels.StaticModels;
using ClothResorting.Models.FBAModels;
using ClothResorting.Controllers.Api.Warehouse;
using ClothResorting.Helpers;

namespace ClothResorting.Manager
{
    public class ItemStatementManager
    {
        private ApplicationDbContext _context;
        private _Application _excel;
        private Workbook _wb;
        private Worksheet _ws;

        public ItemStatementManager(ApplicationDbContext context, string path)
        {
            _context = context;
            _excel = new Application();
            _wb = _excel.Workbooks.Open(path);
        }

        public string GenerateSKUStatement(string customerCode, string sku, DateTime startDate, DateTime endDate)
        {
            var originalStartDate = startDate.ToString("yyyy-MM-dd");
            var originalEndDate = endDate.ToString("yyyy-MM-dd");
            startDate = new DateTime(startDate.Year, startDate.Month, startDate.Day);
            endDate = new DateTime(endDate.Year, endDate.Month, endDate.Day).AddDays(1);
            var index = 5;
            var total = 0;
            var balanceList = new List<ItemStatisticLine>();

            var inboundCollection = _context.FBAOrderDetails
                .Include(x => x.FBAMasterOrder)
                .Where(x => x.ShipmentId == sku
                    && x.FBAMasterOrder.InboundDate >= startDate
                    && x.FBAMasterOrder.InboundDate < endDate
                    && x.FBAMasterOrder.CustomerCode == customerCode)
                .ToList()
                .OrderBy(x => x.FBAMasterOrder.InboundDate);

            var outboundCollection = _context.FBAPickDetailCartons
                .Include(x => x.FBAPickDetail.FBAShipOrder)
                .Include(x => x.FBACartonLocation)
                .Where(x => x.FBACartonLocation.ShipmentId == sku
                    && x.FBAPickDetail.FBAShipOrder.ReleasedDate >= startDate
                    && x.FBAPickDetail.FBAShipOrder.ReleasedDate < endDate
                    && x.FBAPickDetail.FBAShipOrder.CustomerCode == customerCode)
                .ToList()
                .OrderBy(x => x.FBAPickDetail.FBAShipOrder.ReleasedDate);

            // sku inbound statement
            _ws = _wb.Worksheets[1];
            total = 0;
            index = 5;

            _ws.Cells[3, 2] = customerCode;
            _ws.Cells[3, 4] = sku;
            _ws.Cells[3, 6] = originalStartDate;
            _ws.Cells[3, 8] = originalEndDate;

            foreach (var i in inboundCollection)
            {
                total += i.ActualQuantity;
                _ws.Cells[index, 1] = i.ShipmentId;
                _ws.Cells[index, 2] = i.Container;
                _ws.Cells[index, 3] = i.AmzRefId;
                _ws.Cells[index, 4] = i.WarehouseCode;
                _ws.Cells[index, 5] = i.FBAMasterOrder.WarehouseLocation;
                _ws.Cells[index, 6] = i.FBAMasterOrder.InboundDate.ToString("yyyy-MM-dd");
                _ws.Cells[index, 7] = i.ActualQuantity;
                _ws.Cells[index, 8] = total;
                balanceList.Add(new ItemStatisticLine {
                    Reference = i.Container,
                    Container = i.Container,
                    Type = FBAOrderType.Inbound,
                    SKU = i.ShipmentId,
                    AmzRefId = i.AmzRefId,
                    WarehouseCode = i.WarehouseCode,
                    WarehouseLocation = i.FBAMasterOrder.WarehouseLocation,
                    Date = i.FBAMasterOrder.InboundDate,
                    QuantityChange = i.ActualQuantity
                });
                index++;
            }

            // sku outbound statement
            _ws = _wb.Worksheets[2];
            index = 5;
            total = 0;

            _ws.Cells[3, 2] = customerCode;
            _ws.Cells[3, 4] = sku;
            _ws.Cells[3, 6] = originalStartDate;
            _ws.Cells[3, 8] = originalEndDate;

            foreach (var i in outboundCollection)
            {
                total -= i.PickCtns;

                _ws.Cells[index, 1] = i.FBACartonLocation.ShipmentId;
                _ws.Cells[index, 2] = i.FBACartonLocation.Container;
                _ws.Cells[index, 3] = i.FBACartonLocation.AmzRefId;
                _ws.Cells[index, 4] = i.FBACartonLocation.WarehouseCode;
                _ws.Cells[index, 5] = i.FBAPickDetail.FBAShipOrder.WarehouseLocation;
                _ws.Cells[index, 6] = i.FBAPickDetail.FBAShipOrder.ReleasedDate.ToString("yyyy-MM-dd");
                _ws.Cells[index, 7] = -i.PickCtns;
                _ws.Cells[index, 8] = total;

                balanceList.Add(new ItemStatisticLine
                {
                    Reference = i.FBAPickDetail.FBAShipOrder.ShipOrderNumber,
                    Container = i.FBACartonLocation.Container,
                    Type = FBAOrderType.Outbound,
                    SKU = i.FBACartonLocation.ShipmentId,
                    AmzRefId = i.FBACartonLocation.AmzRefId,
                    WarehouseCode = i.FBACartonLocation.WarehouseCode,
                    WarehouseLocation = i.FBAPickDetail.FBAShipOrder.WarehouseLocation,
                    Date = i.FBAPickDetail.FBAShipOrder.ReleasedDate,
                    QuantityChange = -i.PickCtns
                });

                index++;
            }

            // sku balance statement
            _ws = _wb.Worksheets[3];
            total = 0;
            index = 6;
            var balance = 0;
            _ws.Cells[3, 2] = customerCode;
            _ws.Cells[3, 4] = sku;
            _ws.Cells[3, 6] = originalStartDate;
            _ws.Cells[3, 8] = originalEndDate;

            // TO DO 计算在start date之前的balance
            var totalInboundBeforeStartDate = _context.FBAOrderDetails
                                                .Include(x => x.FBAMasterOrder)
                                                .Where(x => x.FBAMasterOrder.InboundDate < startDate && x.ShipmentId == sku)
                                                .ToList()
                                                .Sum(x => x.ActualQuantity);
            var totalOutboundBeforeStartDate = _context.FBAPickDetailCartons
                                                .Include(x => x.FBAPickDetail.FBAShipOrder)
                                                .Include(x => x.FBACartonLocation)
                                                .Where(x => x.FBAPickDetail.FBAShipOrder.ReleasedDate < startDate && x.FBAPickDetail.FBAShipOrder.ReleasedDate.Year != 1900 && x.FBACartonLocation.ShipmentId == sku)
                                                .ToList()
                                                .Sum(x => x.PickCtns);

            balance = totalInboundBeforeStartDate - totalOutboundBeforeStartDate;

            _ws.Cells[5, 1] = sku;
            _ws.Cells[5, 2] = "N/A";
            _ws.Cells[5, 3] = "N/A";
            _ws.Cells[5, 4] = "N/A";
            _ws.Cells[5, 5] = "N/A";
            _ws.Cells[5, 6] = "N/A";
            _ws.Cells[5, 7] = "By end of";
            _ws.Cells[5, 8] = startDate.ToString("yyyy-MM-dd");
            _ws.Cells[5, 9] = balance;
            _ws.Cells[5, 10] = balance;
            balanceList = balanceList.OrderBy(x => x.Date).ToList();
            foreach(var i in balanceList)
            {
                balance += i.QuantityChange;
                _ws.Cells[index, 1] = i.SKU;
                _ws.Cells[index, 2] = i.Reference;
                _ws.Cells[index, 3] = i.Container;
                _ws.Cells[index, 4] = i.AmzRefId;
                _ws.Cells[index, 5] = i.WarehouseCode;
                _ws.Cells[index, 6] = i.WarehouseLocation;
                _ws.Cells[index, 7] = i.Type;
                _ws.Cells[index, 8] = i.Date;
                _ws.Cells[index, 9] = i.QuantityChange;
                _ws.Cells[index, 10] = balance;
                index++;
            }

            var fullPath = @"D:\OtherReport\" +customerCode + "-SKU-" + sku + "-Statement-" + DateTime.Now.ToString("yyyyMMddhhmmssffff") + ".xlsx";
            _wb.SaveAs(fullPath, Type.Missing, "", "", Type.Missing, Type.Missing, XlSaveAsAccessMode.xlNoChange, 1, false, Type.Missing, Type.Missing, Type.Missing);
            _excel.Quit();

            //强行关闭Excel进程
            var killer = new ExcelKiller();
            killer.Dispose();
            return fullPath;
        }

        public string GenerateSKUSummaryStatement(string customerCode, DateTime startDate, DateTime endDate)
        {
            var originalStartDate = startDate.ToString("yyyy-MM-dd");
            var originalEndDate = endDate.ToString("yyyy-MM-dd");
            startDate = new DateTime(startDate.Year, startDate.Month, startDate.Day);
            endDate = new DateTime(endDate.Year, endDate.Month, endDate.Day).AddDays(1);

            var skuList = new List<ItemStatisticLine>();

            var inboundSKUs = _context.FBAOrderDetails
                .Include(x => x.FBAMasterOrder)
                .Where(x => x.FBAMasterOrder.CustomerCode == customerCode
                    && x.FBAMasterOrder.InboundDate >= startDate
                    && x.FBAMasterOrder.InboundDate < endDate)
                .ToList()
                .GroupBy(x => new { x.ShipmentId, x.FBAMasterOrder.WarehouseLocation });

            var outboundSKUs = _context.FBAPickDetailCartons
                .Include(x => x.FBACartonLocation)
                .Include(x => x.FBAPickDetail.FBAShipOrder)
                .Where(x => x.FBAPickDetail.FBAShipOrder.CustomerCode == customerCode
                    && x.FBAPickDetail.FBAShipOrder.ReleasedDate < endDate
                    && x.FBAPickDetail.FBAShipOrder.CustomerCode == customerCode)
                .ToList()
                .GroupBy(x => new { x.FBACartonLocation.ShipmentId, x.FBAPickDetail.FBAShipOrder.WarehouseLocation });

            var totalInboundBeforeStartDate = _context.FBAOrderDetails
                                                .Include(x => x.FBAMasterOrder)
                                                .Where(x => x.FBAMasterOrder.InboundDate < startDate)
                                                .ToList();

            var totalOutboundBeforeStartDate = _context.FBAPickDetailCartons
                                                .Include(x => x.FBAPickDetail.FBAShipOrder)
                                                .Include(x => x.FBACartonLocation)
                                                .Where(x => x.FBAPickDetail.FBAShipOrder.ReleasedDate < startDate && x.FBAPickDetail.FBAShipOrder.ReleasedDate.Year != 1900)
                                                .ToList();

            // 获取所有涉及到进出库的SKU列表
            foreach(var i in inboundSKUs)
            {
                if (!skuList.Where(x => x.SKU == i.First().ShipmentId).Any())
                    skuList.Add(new ItemStatisticLine {
                        SKU = i.First().ShipmentId,
                        WarehouseLocation = i.First().FBAMasterOrder.WarehouseLocation
                    });
            }

            foreach(var o in outboundSKUs)
            {
                if (!skuList.Where(x => x.SKU == o.First().FBACartonLocation.ShipmentId).Any())
                    skuList.Add(new ItemStatisticLine {
                        SKU = o.First().FBACartonLocation.ShipmentId,
                        WarehouseLocation = o.First().FBAPickDetail.FBAShipOrder.WarehouseLocation
                    });
            }

            // 画表
            // sku summary statement
            _ws = _wb.Worksheets[1];
            var index = 5;
            _ws.Cells[3, 2] = customerCode;
            _ws.Cells[3, 4] = originalStartDate;
            _ws.Cells[3, 6] = originalEndDate;

            foreach (var i in skuList)
            {
                var totalInbound = 0;
                var totalOutbouns = 0;
                var openingBalance = totalInboundBeforeStartDate.Where(x => x.ShipmentId == i.SKU).Sum(x => x.ActualQuantity) - totalOutboundBeforeStartDate.Where(x => x.FBACartonLocation.ShipmentId == i.SKU).Sum(x => x.PickCtns);

                var inboundGroup = inboundSKUs.SingleOrDefault(x => x.Key.ShipmentId == i.SKU && x.Key.WarehouseLocation == i.WarehouseLocation);
                var outboundGroup = outboundSKUs.SingleOrDefault(x => x.Key.ShipmentId == i.SKU && x.Key.WarehouseLocation == i.WarehouseLocation);

                if (inboundGroup != null)
                    totalInbound = inboundGroup.Sum(x => x.ActualQuantity);

                if (outboundGroup != null)
                    totalOutbouns = outboundGroup.Sum(x => x.PickCtns);

                _ws.Cells[index, 1] = i.SKU;
                _ws.Cells[index, 2] = i.WarehouseLocation;
                _ws.Cells[index, 3] = openingBalance;
                _ws.Cells[index, 4] = totalInbound;
                _ws.Cells[index, 5] = -totalOutbouns;
                _ws.Cells[index, 6] = openingBalance + totalInbound - totalOutbouns;
                index++;
            }

            var fullPath = @"D:\OtherReport\" + customerCode + "-SKU-Statement-" + DateTime.Now.ToString("yyyyMMddhhmmssffff") + ".xlsx";
            _wb.SaveAs(fullPath, Type.Missing, "", "", Type.Missing, Type.Missing, XlSaveAsAccessMode.xlNoChange, 1, false, Type.Missing, Type.Missing, Type.Missing);
            _excel.Quit();

            //强行关闭Excel进程
            var killer = new ExcelKiller();
            killer.Dispose();
            return fullPath;
        }
    }

    public class ItemStatisticLine
    {
        public string Reference { get; set; }

        public string Container { get; set; }

        public string Type { get; set; }

        public string SKU { get; set; }

        public string AmzRefId { get; set; }

        public string WarehouseCode { get; set; }

        public string WarehouseLocation { get; set; }

        public DateTime Date { get; set; }

        public int QuantityChange { get; set; }
    }
}