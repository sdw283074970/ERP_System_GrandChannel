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
            _ws.Cells[3, 3] = sku;
            _ws.Cells[3, 4] = originalStartDate;
            _ws.Cells[3, 5] = originalEndDate;

            foreach (var i in inboundCollection)
            {
                total += i.ActualQuantity;
                _ws.Cells[index, 1] = i.Container;
                _ws.Cells[index, 2] = i.ShipmentId;
                _ws.Cells[index, 3] = i.AmzRefId;
                _ws.Cells[index, 4] = i.WarehouseCode;
                _ws.Cells[index, 5] = i.FBAMasterOrder.WarehouseLocation;
                _ws.Cells[index, 6] = i.FBAMasterOrder.InboundDate.ToString("yyyy-MM-dd");
                _ws.Cells[index, 7] = i.ActualQuantity;
                _ws.Cells[index, 8] = total;
                balanceList.Add(new ItemStatisticLine {
                    Reference = i.Container,
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
            _ws.Cells[3, 3] = sku;
            _ws.Cells[3, 4] = originalStartDate;
            _ws.Cells[3, 5] = originalEndDate;

            foreach (var i in outboundCollection)
            {
                total -= i.PickCtns;

                _ws.Cells[index, 1] = i.FBACartonLocation.Container;
                _ws.Cells[index, 2] = i.FBACartonLocation.ShipmentId;
                _ws.Cells[index, 3] = i.FBACartonLocation.AmzRefId;
                _ws.Cells[index, 4] = i.FBACartonLocation.WarehouseCode;
                _ws.Cells[index, 5] = i.FBAPickDetail.FBAShipOrder.WarehouseLocation;
                _ws.Cells[index, 6] = i.FBAPickDetail.FBAShipOrder.ReleasedDate.ToString("yyyy-MM-dd");
                _ws.Cells[index, 7] = -i.PickCtns;
                _ws.Cells[index, 8] = total;

                balanceList.Add(new ItemStatisticLine
                {
                    Reference = i.FBAPickDetail.FBAShipOrder.ShipOrderNumber,
                    Type = FBAOrderType.Outbound,
                    SKU = i.FBACartonLocation.ShipmentId,
                    AmzRefId = i.FBACartonLocation.AmzRefId,
                    WarehouseCode = i.FBACartonLocation.WarehouseCode,
                    WarehouseLocation = i.FBAPickDetail.FBAShipOrder.WarehouseLocation,
                    Date = i.FBAPickDetail.FBAShipOrder.ReleasedDate,
                    QuantityChange = i.PickCtns
                });

                index++;
            }

            // sku balance statement
            _ws = _wb.Worksheets[3];
            total = 0;
            index = 6;
            var balance = 0;
            _ws.Cells[3, 2] = customerCode;
            _ws.Cells[3, 3] = sku;
            _ws.Cells[3, 4] = originalStartDate;
            _ws.Cells[3, 5] = originalEndDate;

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

            _ws.Cells[5, 1] = "N/A";
            _ws.Cells[5, 2] = sku;
            _ws.Cells[5, 3] = "N/A";
            _ws.Cells[5, 4] = "N/A";
            _ws.Cells[5, 5] = "N/A";
            _ws.Cells[5, 6] = "By end of";
            _ws.Cells[5, 7] = startDate.ToString("yyyy-MM-dd");
            _ws.Cells[5, 8] = balance;
            _ws.Cells[5, 9] = balance;

            foreach(var i in balanceList)
            {
                balance += i.QuantityChange;
                _ws.Cells[index, 1] = i.Reference;
                _ws.Cells[index, 2] = i.SKU;
                _ws.Cells[index, 3] = i.AmzRefId;
                _ws.Cells[index, 4] = i.WarehouseCode;
                _ws.Cells[index, 5] = i.WarehouseLocation;
                _ws.Cells[index, 6] = i.Type;
                _ws.Cells[index, 7] = i.Date;
                _ws.Cells[index, 8] = i.QuantityChange;
                _ws.Cells[index, 9] = balance;
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
    }

    public class ItemStatisticLine
    {
        public string Reference { get; set; }

        public string Type { get; set; }

        public string SKU { get; set; }

        public string AmzRefId { get; set; }

        public string WarehouseCode { get; set; }

        public string WarehouseLocation { get; set; }

        public DateTime Date { get; set; }

        public int QuantityChange { get; set; }
    }
}