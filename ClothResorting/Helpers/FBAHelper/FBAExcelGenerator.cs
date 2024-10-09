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

namespace ClothResorting.Helpers.FBAHelper
{
    public class FBAExcelGenerator
    {
        private ApplicationDbContext _context;
        private string _path = "";
        private _Application _excel;
        private Workbook _wb;
        private Worksheet _ws;

        public FBAExcelGenerator()
        {
            _context = new ApplicationDbContext();
        }

        public FBAExcelGenerator(string templatePath)
        {
            _context = new ApplicationDbContext();
            _path = templatePath;
            _excel = new Application();
            _wb = _excel.Workbooks.Open(_path);
        }

        //生成Receipt文件并返回完整路径
        public string GenerateReceipt(int masterOrderId)
        {
            var masterOrderInDb = _context.FBAMasterOrders
                .Include(x => x.FBAOrderDetails)
                .Include(x => x.Customer)
                .SingleOrDefault(x => x.Id == masterOrderId);

            var cartonLocations = _context.FBACartonLocations
                .Include(x => x.FBAOrderDetail.FBAMasterOrder)
                .Where(x => x.Location != "Pallet" && x.FBAOrderDetail.FBAMasterOrder.Id == masterOrderId);

            var unPalletizedSKU = masterOrderInDb.FBAOrderDetails
                .Where(x => x.ComsumedQuantity < x.ActualQuantity);

            var pallets = _context.FBAPallets
                .Include(x => x.FBAMasterOrder)
                .Include(x => x.FBACartonLocations)
                .Where(x => x.FBAMasterOrder.Id == masterOrderId && x.ActualPallets != 0)
                .ToList();

            var totalPlts = pallets == null ? 0 : pallets.Sum(x => x.ActualPallets);

            // 接收报告
            _ws = _wb.Worksheets[1];
            var startRow = 5;

            foreach (var d in masterOrderInDb.FBAOrderDetails)
            {
                _ws.Cells[startRow, 1] = d.ShipmentId;
                _ws.Cells[startRow, 2] = d.AmzRefId;
                _ws.Cells[startRow, 3] = d.Quantity;
                _ws.Cells[startRow, 4] = d.ActualQuantity;
                _ws.Cells[startRow, 5] = d.WarehouseCode;

                startRow += 1;
            }

            // 写入表头信息
            _ws.Cells[2, 2] = masterOrderInDb.Customer.CustomerCode;
            _ws.Cells[2, 4] = masterOrderInDb.InboundDate.ToString("yyyy-MM-dd");
            _ws.Cells[3, 2] = masterOrderInDb.Container;
            _ws.Cells[3, 4] = masterOrderInDb.OriginalPlts;
            _ws.Cells[3, 5] = $"{masterOrderInDb.DamagedBox} Ctns Dmg";

            // 写入表脚信息
            _ws.Cells[startRow, 1] = "Total:";
            _ws.Cells[startRow, 3] = masterOrderInDb == null ? 0 : masterOrderInDb.FBAOrderDetails.Sum(x => x.Quantity);
            _ws.Cells[startRow, 4] = masterOrderInDb == null ? 0 : masterOrderInDb.FBAOrderDetails.Sum(x => x.ActualQuantity);

            //加上边框
            var range = _ws.get_Range("A5", "E" + startRow);
            range.Borders.LineStyle = 1;

            // 打托报告
            _ws = _wb.Worksheets[2];
            var ws = (Worksheet)_wb.ActiveSheet;

            startRow = 5;

            // 写入打托的SKU
            foreach (var p in pallets)
            {
                //合并单元格
                _ws.Cells[startRow, 1] = "Palletized";
                _ws.Cells[startRow, 6] = p.PalletSize;
                _ws.Cells[startRow, 7] = p.ActualPallets;

                var statusRange = _ws.get_Range("A" + startRow, "A" + (startRow + p.FBACartonLocations.Count - 1));
                var pltSizeRange = _ws.get_Range("F" + startRow, "F" + (startRow + p.FBACartonLocations.Count - 1));
                var palletsRange = _ws.get_Range("G" + startRow, "G" + (startRow + p.FBACartonLocations.Count - 1));
                statusRange.Merge(statusRange.MergeCells);
                pltSizeRange.Merge(pltSizeRange.MergeCells);
                palletsRange.Merge(palletsRange.MergeCells);

                foreach (var c in p.FBACartonLocations)
                {
                    _ws.Cells[startRow, 2] = c.ShipmentId;
                    _ws.Cells[startRow, 3] = c.AmzRefId;
                    _ws.Cells[startRow, 4] = c.FBAOrderDetail.ActualQuantity;
                    _ws.Cells[startRow, 5] = c.ActualQuantity + " / " + c.FBAOrderDetail.ActualQuantity;
                    _ws.Cells[startRow, 8] = c.WarehouseCode;
                    startRow += 1;
                }
            }

            // 写入未打托但直接分派了库位的SKU
            foreach(var c in cartonLocations)
            {
                _ws.Cells[startRow, 1] = "Unpalletized";
                _ws.Cells[startRow, 2] = c.ShipmentId;
                _ws.Cells[startRow, 3] = c.AmzRefId;
                _ws.Cells[startRow, 4] = c.FBAOrderDetail.ActualQuantity;
                _ws.Cells[startRow, 5] = c.ActualQuantity + " / " + c.FBAOrderDetail.ActualQuantity;
                _ws.Cells[startRow, 6] = "N/A";
                _ws.Cells[startRow, 7] = "N/A";
                _ws.Cells[startRow, 8] = c.WarehouseCode;
                startRow += 1;
            }

            // 写入未打托且未直接入库的SKU
            foreach (var s in unPalletizedSKU)
            {
                _ws.Cells[startRow, 1] = "Pending";
                _ws.Cells[startRow, 2] = s.ShipmentId;
                _ws.Cells[startRow, 3] = s.AmzRefId;
                _ws.Cells[startRow, 4] = s.ActualQuantity;
                _ws.Cells[startRow, 5] = s.ActualQuantity - s.ComsumedQuantity + " / " + s.ActualQuantity;
                _ws.Cells[startRow, 6] = "N/A";
                _ws.Cells[startRow, 7] = "N/A";
                _ws.Cells[startRow, 8] = s.WarehouseCode;
                startRow += 1;
            }

            // 写入表头信息
            _ws.Cells[2, 2] = masterOrderInDb.Customer.CustomerCode;
            _ws.Cells[2, 4] = masterOrderInDb.InboundDate.ToString("yyyy-MM-dd");
            _ws.Cells[3, 2] = masterOrderInDb.Container;
            _ws.Cells[3, 4] = masterOrderInDb.OriginalPlts;

            //写入表脚信息
            _ws.Cells[startRow, 1] = "Total:";
            _ws.Cells[startRow, 5] = masterOrderInDb == null ? 0 : masterOrderInDb.FBAOrderDetails.Sum(x => x.ActualQuantity) + " / " + masterOrderInDb == null ? 0 : masterOrderInDb.FBAOrderDetails.Sum(x => x.ActualQuantity);
            _ws.Cells[startRow, 7] = pallets == null ? 0 : pallets.Sum(x => x.ActualPallets);

            //加上边框
            range = _ws.get_Range("A5", "H" + startRow);
            range.Borders.LineStyle = 1;

            var fullPath = @"E:\Receipts\FBA-" + masterOrderInDb.Customer.CustomerCode + "-Receipt-" + masterOrderInDb.Container + "-" + DateTime.Now.ToString("yyyyMMddhhmmssffff") + ".xlsx";
            _wb.SaveAs(fullPath, Type.Missing, "", "", Type.Missing, Type.Missing, XlSaveAsAccessMode.xlNoChange, 1, false, Type.Missing, Type.Missing, Type.Missing);

            _excel.Quit();

            //强行关闭Excel进程
            var killer = new ExcelKiller();
            killer.Dispose();

            return fullPath;
        }

        //生成StorageFee报告并返回完整路径
        public string GenerateStorageReport(int customerId, DateTime startDate, DateTime closeDate, float p1Discount, float p2Discount, string[] warehouseLocations, bool includePrereleasedOrder)
        {
            var customerInDb = _context.UpperVendors.Find(customerId);

            var palletLocationInDb = _context.FBAPalletLocations
                .Include(x => x.FBAMasterOrder.Customer)
                .Include(x => x.FBAMasterOrder.FBAPallets)
                .Include(x => x.FBAPickDetails.Select(c => c.FBAShipOrder))
                .Where(x => x.FBAMasterOrder.InboundDate < closeDate
                    && x.FBAMasterOrder.Customer.Id == customerId)
                .Where(x => warehouseLocations.Contains(x.FBAMasterOrder.WarehouseLocation));

            IQueryable<FBAPickDetail> pickDetailInDb;

            if (!includePrereleasedOrder)
            {
                pickDetailInDb = _context.FBAPickDetails
                                .Include(x => x.FBAPalletLocation.FBAMasterOrder.Customer)
                                .Include(x => x.FBAShipOrder)
                                .Include(x => x.FBAPickDetailCartons)
                                .Where(x => x.FBAShipOrder.ShipDate < closeDate
                                    && x.FBAShipOrder.ShipDate >= startDate
                                    && x.FBAShipOrder.Status == FBAStatus.Shipped
                                    && x.FBAPalletLocation != null
                                    && x.FBAPalletLocation.FBAMasterOrder.InboundDate < closeDate
                                    && x.FBAPalletLocation.FBAMasterOrder.Customer.Id == customerId
                                    && x.PltsFromInventory != 0)
                                .Where(x => warehouseLocations.Contains(x.FBAShipOrder.WarehouseLocation));
            }
            else
            {
                pickDetailInDb = _context.FBAPickDetails
                                .Include(x => x.FBAPalletLocation.FBAMasterOrder.Customer)
                                .Include(x => x.FBAShipOrder)
                                .Include(x => x.FBAPickDetailCartons)
                                .Where(x => ((x.FBAShipOrder.ReleasedDate < closeDate && x.FBAShipOrder.ReleasedDate >= startDate)
                                    || (x.FBAShipOrder.ShipDate < closeDate && x.FBAShipOrder.ShipDate >= startDate))
                                    && (x.FBAShipOrder.Status == FBAStatus.Shipped || (x.FBAShipOrder.Status == FBAStatus.Released && x.FBAShipOrder.IsPrereleasing))
                                    && x.FBAPalletLocation != null
                                    && x.FBAPalletLocation.FBAMasterOrder.InboundDate < closeDate
                                    && x.FBAPalletLocation.FBAMasterOrder.Customer.Id == customerId
                                    && x.PltsFromInventory != 0)
                                .Where(x => warehouseLocations.Contains(x.FBAShipOrder.WarehouseLocation));

                //pickDetailInDb = _context.FBAPickDetails
                //                .Include(x => x.FBAPalletLocation.FBAMasterOrder.Customer)
                //                .Include(x => x.FBAShipOrder)
                //                .Where(x => x.FBAShipOrder.ReleasedDate < closeDate
                //                    && x.FBAShipOrder.ReleasedDate >= startDate
                //                    && x.FBAShipOrder.Status == FBAStatus.Released
                //                    && x.FBAShipOrder.IsPrereleasing == true
                //                    && x.FBAPalletLocation != null
                //                    && x.FBAPalletLocation.FBAMasterOrder.InboundDate < closeDate
                //                    && x.FBAPalletLocation.FBAMasterOrder.Customer.Id == customerId
                //                    && x.PltsFromInventory != 0)
                //                .Where(x => warehouseLocations.Contains(x.FBAShipOrder.WarehouseLocation));
            }

            //var cartonLocationInDb = _context.FBACartonLocations
            //    .Include(x => x.FBAOrderDetail.FBAMasterOrder.Customer)
            //    .Where(x => x.FBAOrderDetail.FBAMasterOrder.InboundDate <= closeDate 
            //        && x.FBAOrderDetail.FBAMasterOrder.Customer.Id == customerId);

            foreach (var p in palletLocationInDb)
            {
                foreach(var pick in p.FBAPickDetails)
                {
                    if (!includePrereleasedOrder)
                    {
                        //从原有托盘数量扣除状态为shipped状态，且发货日期在结束日期之前的运单中的托盘数量
                        if (pick.FBAShipOrder.Status == FBAStatus.Shipped && pick.FBAShipOrder.ShipDate < closeDate)
                        {
                            p.ActualPlts -= pick.PltsFromInventory;
                            p.ActualQuantity -= pick.PickableCtns;
                        }
                    }
                    else
                    {
                        //从原有托盘数量扣除状态为shipped和pre-releasing状态，且发货日期、预发货日期在结束日期之前的运单中的托盘数量
                        if ((pick.FBAShipOrder.Status == FBAStatus.Shipped || (pick.FBAShipOrder.Status == FBAStatus.Released && pick.FBAShipOrder.IsPrereleasing)) && pick.FBAShipOrder.ShipDate < closeDate && pick.FBAShipOrder.ReleasedDate < closeDate)
                        {
                            p.ActualPlts -= pick.PltsFromInventory;
                            p.ActualQuantity -= pick.PickableCtns;
                        }
                    }
                }
            }

            _ws = _wb.Worksheets[1];

            var startIndex = 2;

            var palletsInDbGroup = palletLocationInDb.GroupBy(x => x.FBAMasterOrder.Container);

            //对仓库剩余托盘进行收费
            foreach(var p in palletsInDbGroup)
            {
                //var pallets = p.Sum(x => x.ActualPlts);
                var standardPlts = p.Where(x => x.PalletSize == "P1").Sum(x => x.ActualPlts);
                var plusPlts = p.Where(x => x.PalletSize == "P2").Sum(x => x.ActualPlts);
                var pallets = Math.Round(standardPlts * p1Discount + 2 * plusPlts * p2Discount, 2);
                //if (pallets == 0)
                //{
                //    continue;
                //}

                _ws.Cells[startIndex, 1] = FBAOrderType.MasterOrder;
                _ws.Cells[startIndex, 2] = p.First().Container;
                _ws.Cells[startIndex, 3] = p.First().FBAMasterOrder.FBAPallets.Where(x => x.PalletSize == "P1").Sum(x => x.ActualPallets);
                _ws.Cells[startIndex, 4] = p.First().FBAMasterOrder.FBAPallets.Where(x => x.PalletSize == "P2").Sum(x => x.ActualPallets);
                _ws.Cells[startIndex, 5] = standardPlts;
                _ws.Cells[startIndex, 6] = plusPlts;
                _ws.Cells[startIndex, 7] = pallets;
                _ws.Cells[startIndex, 8] = p.First().FBAMasterOrder.InboundDate.ToString("MM/dd/yyyy");
                _ws.Cells[startIndex, 13] = p.First().FBAMasterOrder.WarehouseLocation;
                _ws.Cells[startIndex, 14] = p.First().FBAMasterOrder.SubCustomer;
                _ws.Cells[startIndex, 16] = p.First().FBAMasterOrder.FBAPallets.Sum(x => x.ActualQuantity);
                _ws.Cells[startIndex, 17] = p.Sum(x => x.ActualQuantity);

                startIndex += 1;
            }

            //对每一运单进行收费
            var shipList = new List<ShipRecord>();
            foreach(var s in pickDetailInDb)
            {
                var newShipRecord = new ShipRecord {
                    Reference = s.FBAShipOrder.ShipOrderNumber,
                    ShippedPlts = s.PltsFromInventory,
                    InboundDate = s.FBAPalletLocation.FBAMasterOrder.InboundDate.ToString("MM/dd/yyyy"),
                    OutboundDate = s.FBAShipOrder.ShipDate.ToString("MM/dd/yyyy"),
                    PalletSize = s.Size,
                    subCustomer = s.FBAShipOrder.SubCustomer,
                    WarehouseLocation = s.FBAShipOrder.WarehouseLocation,
                    ShippedCtns = s.FBAPickDetailCartons.Sum(x => x.PickCtns)
                };

                if (includePrereleasedOrder && s.FBAShipOrder.Status == FBAStatus.Released && s.FBAShipOrder.IsPrereleasing)
                {
                    newShipRecord.OutboundDate = s.FBAShipOrder.ReleasedDate.ToString("MM/dd/yyyy");
                    newShipRecord.IsPreReleased = true;
                }

                var sameShipRecord = shipList.SingleOrDefault(x => x.Reference == newShipRecord.Reference
                    && x.InboundDate == newShipRecord.InboundDate
                    && x.OutboundDate == newShipRecord.OutboundDate
                    && x.PalletSize == newShipRecord.PalletSize);

                if (sameShipRecord == null)
                {
                    shipList.Add(newShipRecord);
                }
                else
                {
                    sameShipRecord.ShippedPlts += newShipRecord.ShippedPlts;
                }
            }

            foreach(var s in shipList)
            {
                if (s.PalletSize == "P1")
                {
                    _ws.Cells[startIndex, 5] = s.ShippedPlts;
                    _ws.Cells[startIndex, 7] = Math.Round(s.ShippedPlts *  p1Discount, 4);
                }
                else if (s.PalletSize == "P2")
                {
                    _ws.Cells[startIndex, 6] = s.ShippedPlts;
                    _ws.Cells[startIndex, 7] = Math.Round(s.ShippedPlts * 2 * p2Discount, 4);
                }
                else
                {
                    _ws.Cells[startIndex, 7] = "Invalid pallet size";
                }

                _ws.Cells[startIndex, 17] = s.ShippedCtns;

                _ws.Cells[startIndex, 1] = FBAOrderType.ShipOrder;
                _ws.Cells[startIndex, 2] = s.Reference;
                _ws.Cells[startIndex, 8] = s.InboundDate;
                _ws.Cells[startIndex, 9] = s.OutboundDate;
                _ws.Cells[startIndex, 13] = s.WarehouseLocation;
                _ws.Cells[startIndex, 14] = s.subCustomer;

                if (s.IsPreReleased)
                    _ws.Cells[startIndex, 14] = "Pre-released";

                startIndex += 1;
            }

            if (p1Discount != 1)
                _ws.Cells[startIndex, 5] = (1 - p1Discount) * 100 + "%OFF";

            if (p2Discount != 1)
                _ws.Cells[startIndex, 6] = Math.Round((1 - p2Discount), 4) * 100 + "%OFF";

            var range = _ws.get_Range("A1", "L" + (startIndex + 1));
            range.HorizontalAlignment = XlHAlign.xlHAlignCenter;
            range.VerticalAlignment = XlHAlign.xlHAlignCenter;

            var fullPath = @"E:\StorageFee\FBA-" + customerInDb.CustomerCode + "-StorageFee-" + DateTime.Now.ToString("yyyyMMddhhmmssffff") + ".xlsx";
            _wb.SaveAs(fullPath, Type.Missing, "", "", Type.Missing, Type.Missing, XlSaveAsAccessMode.xlNoChange, 1, false, Type.Missing, Type.Missing, Type.Missing);

            _excel.Quit();

            return fullPath;
        }

        //生成拣货单并返回完整路径
        public string GenerateShippingWOAndPickingList(int shipOrderId)
        {
            var woHelper = new FBAWOHelper();
            //第一页生成WO
            _ws = _wb.Worksheets[1];

            var shipOrderInDb = _context.FBAShipOrders
                .Include(x => x.ChargingItemDetails)
                .Include(x => x.FBAPickDetails)
                .SingleOrDefault(x => x.Id == shipOrderId);

            if (shipOrderInDb.PlaceTime.Year == 1900)
            {
                _ws.Cells[2, 1] = "Order Create Date";
                _ws.Cells[2, 2] = shipOrderInDb.CreateDate.ToString("yyyy-MM-dd");
            }
            else
            {
                _ws.Cells[2, 1] = "Order Place Date";
                _ws.Cells[2, 2] = shipOrderInDb.PlaceTime.ToString("yyyy-MM-dd");
            }

            _ws.Cells[2, 6] = shipOrderInDb.CustomerCode + ", " + shipOrderInDb.ETSTimeRange;
            _ws.Cells[3, 2] = shipOrderInDb.ShipOrderNumber;
            _ws.Cells[3, 6] = shipOrderInDb.ETS.ToString("yyyy-MM-dd");
            _ws.Cells[4, 2] = shipOrderInDb.Destination ?? "NA";
            _ws.Cells[5, 5] = shipOrderInDb.FBAPickDetails.Sum(x => x.ActualQuantity);
            _ws.Cells[5, 7] = shipOrderInDb.FBAPickDetails.Sum(x => x.PltsFromInventory);
            _ws.Cells[5, 2] = shipOrderInDb.Carrier;

            var instructionList = shipOrderInDb.ChargingItemDetails
                .Where(x => x.HandlingStatus != "N/A")
                .ToList();

            for (int i = 0; i < instructionList.Count; i++)
            {
                _ws.Cells[i + 7, 1] = (i + 1).ToString() + ". " + instructionList[i].Description;
            }

            // 第二页生成SKU合并版Picking List
            _ws = _wb.Worksheets[2];
            var startIndex = 3;

            var pickDetailInDb = _context.FBAPickDetails
                .Include(x => x.FBAShipOrder)
                .Include(x => x.FBAPickDetailCartons)
                .Include(x => x.FBAPalletLocation.FBAPallet.FBACartonLocations)
                .Where(x => x.FBAShipOrder.Id == shipOrderId)
                .ToList();

            // 如果没有拣货项目那么第二页就什么都不生成
            if (pickDetailInDb.Count == 0)
            {
                _ws.Cells[1, 2] = "No need to pick";
            }
            else
            {
                _ws.Cells[1, 2] = pickDetailInDb.First().FBAShipOrder.ShipOrderNumber.ToString();
                //var bolList = GenerateFBABOLList(pickDetailInDb);
                var bolList = woHelper.GenerateFBABOLList(pickDetailInDb);
                var groupLis = bolList.GroupBy(x => x.ParentPalletId);

                foreach (var g in groupLis)
                {
                    var startRow = startIndex;
                    _ws.Cells[startIndex, 3] = g.First().Contianer;
                    _ws.Cells[startIndex, 5] = g.First().ParentPalletId == 0 ? "NA" : g.First().PickPallets.ToString();
                    _ws.Cells[startIndex, 6] = g.First().Location;

                    foreach (var p in g)
                    {
                        _ws.Cells[startIndex, 1] = p.CustomerOrderNumber;
                        _ws.Cells[startIndex, 2] = p.AmzRef;
                        _ws.Cells[startIndex, 4] = p.CartonQuantity;
                        _ws.Cells[startIndex, 7] = p.Memo;

                        if (p.ParentPalletId == 0)
                        {
                            _ws.Cells[startIndex, 3] = p.Contianer;
                            _ws.Cells[startIndex, 6] = p.Location;
                            _ws.Cells[startIndex, 7] = p.Memo;
                        }

                        startIndex += 1;
                    }

                    if (g.Count() > 1) //当组内数量大于1才有纵向合并单元格的必要
                    {
                        if (g.First().ParentPalletId != 0) //按托拣货
                        {
                            var rangeContainer = _ws.get_Range("C" + startRow, "C" + (startIndex - 1));
                            rangeContainer.Merge(rangeContainer.MergeCells);

                            var rangeLocation = _ws.get_Range("F" + startRow, "F" + (startIndex - 1));
                            rangeLocation.Merge(rangeLocation.MergeCells);
                        }

                        var rangePlts = _ws.get_Range("E" + startRow, "E" + (startIndex - 1));
                        rangePlts.Merge(rangePlts.MergeCells);
                    }
                }

                _ws.Cells[startIndex + 1, 3] = "Total";
                _ws.Cells[startIndex + 1, 4] = pickDetailInDb.Sum(x => x.ActualQuantity);
                _ws.Cells[startIndex + 1, 5] = pickDetailInDb.Sum(x => x.ActualPlts);

                var range = _ws.get_Range("A2:G" + (startIndex + 1), Type.Missing);

                range.HorizontalAlignment = XlVAlign.xlVAlignCenter;
                range.VerticalAlignment = XlVAlign.xlVAlignCenter;
                range.Borders.LineStyle = 1;
            }

            // 第三页生成非合并版Picking List
            _ws = _wb.Worksheets[3];
            var pickDetailsCartonsInDb = _context.FBAPickDetailCartons
                .Include(x => x.FBAPickDetail.FBAShipOrder)
                .Include(x => x.FBACartonLocation)
                .Where(x => x.FBAPickDetail.FBAShipOrder.Id == shipOrderId)
                .ToList();

            startIndex = 3;

            // 如果没有拣货项目那么第三页就什么都不生成
            if (pickDetailInDb.Count == 0)
            {
                _ws.Cells[1, 2] = "No need to pick";
            }
            else
            {
                _ws.Cells[1, 2] = pickDetailInDb.First().FBAShipOrder.ShipOrderNumber.ToString();
                var groupLis = pickDetailsCartonsInDb.GroupBy(x => x.FBAPickDetail.Id);

                foreach (var g in groupLis)
                {
                    var startRow = startIndex;
                    _ws.Cells[startIndex, 3] = g.First().FBAPickDetail.Container;
                    _ws.Cells[startIndex, 5] = g.First().FBAPickDetail.PltsFromInventory.ToString();
                    _ws.Cells[startIndex, 6] = g.First().FBAPickDetail.Location;

                    foreach (var p in g)
                    {
                        _ws.Cells[startIndex, 1] = p.FBACartonLocation.ShipmentId;
                        _ws.Cells[startIndex, 2] = p.FBACartonLocation.AmzRefId;
                        _ws.Cells[startIndex, 4] = p.PickCtns;
                        _ws.Cells[startIndex, 7] = p.FBACartonLocation.Memo;

                        startIndex += 1;
                    }

                    if (g.Count() > 1) //当组内数量大于1才有纵向合并单元格的必要
                    {
                        var rangeContainer = _ws.get_Range("C" + startRow, "C" + (startIndex - 1));
                        rangeContainer.Merge(rangeContainer.MergeCells);

                        var rangePlts = _ws.get_Range("F" + startRow, "F" + (startIndex - 1));
                        rangePlts.Merge(rangePlts.MergeCells);
                    }
                }

                _ws.Cells[startIndex + 1, 3] = "Total";
                _ws.Cells[startIndex + 1, 4] = pickDetailInDb.Sum(x => x.ActualQuantity);
                _ws.Cells[startIndex + 1, 5] = pickDetailInDb.Sum(x => x.ActualPlts);

                var range = _ws.get_Range("A2:G" + (startIndex + 1), Type.Missing);

                range.HorizontalAlignment = XlVAlign.xlVAlignCenter;
                range.VerticalAlignment = XlVAlign.xlVAlignCenter;
                range.Borders.LineStyle = 1;
            }

            var fullPath = @"E:\PickingList\" + shipOrderInDb.CustomerCode + "-OB-WO-" + shipOrderInDb.ShipOrderNumber + "-PL" + DateTime.Now.ToString("yyyyMMddhhmmssffff") + ".xlsx";

            _wb.SaveAs(fullPath, Type.Missing, "", "", Type.Missing, Type.Missing, XlSaveAsAccessMode.xlNoChange, 1, false, Type.Missing, Type.Missing, Type.Missing);

            _excel.Quit();

            return fullPath;
        }

        public string GenerateUnloadingWOAndPackingList(int masterOrderId)
        {
            //第一页生成WO
            _ws = _wb.Worksheets[1];

            var masterOrder = _context.FBAMasterOrders
                .Include(x => x.FBAOrderDetails)
                .Include(x => x.ChargingItemDetails)
                .SingleOrDefault(x => x.Id == masterOrderId);

            _ws.Cells[2, 4] = masterOrder.Container;
            _ws.Cells[3, 4] = masterOrder.GrandNumber;
            _ws.Cells[4, 4] = masterOrder.FBAOrderDetails.Sum(x => x.Quantity);
            _ws.Cells[4, 9] = masterOrder.FBAOrderDetails.GroupBy(x => x.ShipmentId).Count();
            _ws.Cells[5, 4] = masterOrder.OriginalPlts;
            _ws.Cells[5, 9] = masterOrder.ETA;
            _ws.Cells[6, 9] = masterOrder.ContainerSize;
            _ws.Cells[7, 4] = masterOrder.UnloadingType;
            _ws.Cells[7, 9] = masterOrder.StorageType;

            var instructionList = masterOrder.ChargingItemDetails
                .Where(x => x.HandlingStatus != "N/A")
                .ToList();

            for (int i = 0; i < instructionList.Count; i++)
            {
                _ws.Cells[i + 9, 1] = (i + 1).ToString() + ". " + instructionList[i].Description;
            }

            //第二页生成Packing List
            _ws = _wb.Worksheets[2];
            var startIndex = 4;
            var orderDetails = masterOrder.FBAOrderDetails;
            _ws.Cells[2, 2] = masterOrder.Container;

            foreach(var o in orderDetails)
            {
                float kgs = (float)Math.Round(o.GrossWeight, 2);
                float lbsPerCtn = (float)(kgs * 2.2 / o.Quantity);
                int sgtCtnsPerplt = 0;

                if (lbsPerCtn != 0)
                    sgtCtnsPerplt = (int)(1500 / lbsPerCtn);

                _ws.Cells[startIndex, 1] = o.ShipmentId;
                _ws.Cells[startIndex, 2] = o.AmzRefId;
                _ws.Cells[startIndex, 3] = o.WarehouseCode;
                _ws.Cells[startIndex, 4] = kgs;
                _ws.Cells[startIndex, 5] = Math.Round(o.CBM, 2);
                _ws.Cells[startIndex, 6] = o.Quantity;
                _ws.Cells[startIndex, 10] = o.Remark;
                _ws.Cells[startIndex, 11] = sgtCtnsPerplt;
                startIndex++;
            }

            _ws.Cells[startIndex + 1, 1] = "Total";
            _ws.Cells[startIndex + 1, 4] = Math.Round(orderDetails.Sum(x => x.GrossWeight), 2);
            _ws.Cells[startIndex + 1, 5] = Math.Round(orderDetails.Sum(x => x.CBM), 2);
            _ws.Cells[startIndex + 1, 6] = orderDetails.Sum(x => x.Quantity);

            var range = _ws.get_Range("A1:K" + (startIndex + 1), Type.Missing);

            range.HorizontalAlignment = XlVAlign.xlVAlignCenter;
            range.VerticalAlignment = XlVAlign.xlVAlignCenter;
            range.Borders.LineStyle = 1;

            var fullPath = @"E:\PickingList\" + masterOrder.CustomerCode + "-IB-WO-" + masterOrder.Container + "-PL" + DateTime.Now.ToString("yyyyMMddhhmmssffff") + ".xlsx";
            _wb.SaveAs(fullPath, Type.Missing, "", "", Type.Missing, Type.Missing, XlSaveAsAccessMode.xlNoChange, 1, false, Type.Missing, Type.Missing, Type.Missing);
            _excel.Quit();
            return fullPath;
        }

        //生成Excel版本的BOL并返回完整路径
        public string GenerateExcelBol(int referenceId, string orderType, IList<FBABOLDetail> bolDetailList, string freightCharge, BOLDetail bolDetail)
        {
            var orderNumber = "N/A";
            var address = " ";
            _ws = _wb.Worksheets[1];

            //设置BOL时间
            _ws.Cells[2, 1] = "Date: " + DateTime.Now.ToString("yyyy-MM-dd");

            if (orderType == FBAOrderType.ShipOrder)
            {
                var orderInDb = _context.FBAShipOrders.Find(referenceId);
                var addressBookInDb = _context.FBAAddressBooks.SingleOrDefault(x => x.WarehouseCode == orderInDb.Destination);
                var warehouseLocationInDb = _context.WarehouseLocations.SingleOrDefault(x => x.WarehouseCode == orderInDb.WarehouseLocation);

                if (addressBookInDb != null)
                {
                    address = addressBookInDb.Address;
                }

                // 设置BOL提货地址
                _ws.Cells[4, 1] = "Grand Channel Inc. " + orderInDb.WarehouseLocation;
                _ws.Cells[5, 1] = warehouseLocationInDb.Address;

                //设置BOL#
                _ws.Cells[3, 6] = orderInDb.BOLNumber;

                //设置地址
                _ws.Cells[7, 2] = orderInDb.Destination;
                _ws.Cells[8, 1] = address;

                //设置carrier
                _ws.Cells[6, 6] = orderInDb.Carrier;

                orderNumber = orderInDb.ShipOrderNumber;

                //设置Ship Order #
                _ws.Cells[18, 1] = "Ship Order#: " + orderNumber;
            }
            else
            {
                var orderInDb = _context.FBAMasterOrders.Find(referenceId);
                var warehouseLocationInDb = _context.WarehouseLocations.SingleOrDefault(x => x.WarehouseCode == orderInDb.WarehouseLocation);

                // 设置BOL提货地址
                _ws.Cells[4, 1] = "Grand Channel Inc. " + orderInDb.WarehouseLocation;
                _ws.Cells[5, 1] = warehouseLocationInDb.Address;

                //设置BOL#
                _ws.Cells[3, 6] = bolDetail.BOLNumber;

                //设置地址
                _ws.Cells[7, 2] = bolDetail.WarehouseCode;
                _ws.Cells[8, 1] = bolDetail.Address;

                //设置carrier
                _ws.Cells[6, 6] = bolDetail.Carrier;

                orderNumber = bolDetail.BOLNumber;

                //设置Ship Order #
                _ws.Cells[18, 1] = "Ship Order#: " + orderNumber;
            }

            //设置Freight Charge
            if (freightCharge == "Prepaid")
            {
                _ws.Cells[16, 4] = "Prepaid ☑  Collect ☐  3rd Party ☐";
            }
            else if (freightCharge == "Collect")
            {
                _ws.Cells[16, 4] = "Prepaid ☐  Collect ☑  3rd Party ☐";
            }
            else if (freightCharge == "3rd Party")
            {
                _ws.Cells[16, 4] = "Prepaid ☐  Collect ☐  3rd Party ☑";
            }
            else
            {
                _ws.Cells[16, 4] = "Prepaid ☐  Collect ☐  3rd Party ☐";
            }

            var startRow = 21;
            var mergeStartRow = 21;
            var mergeEndRow = 21;
            var mergeRange = _ws.get_Range("G" + mergeStartRow, "G" + mergeEndRow);

            foreach (var b in bolDetailList)
            {
                _ws.Cells[startRow, 1] = b.CustomerOrderNumber;
                _ws.Cells[startRow, 1].HorizontalAlignment = XlHAlign.xlHAlignCenter;

                _ws.Cells[startRow, 2] = b.Contianer;
                _ws.Cells[startRow, 3] = b.AmzRef;
                _ws.Cells[startRow, 5] = Math.Round(b.Weight, 2);
                _ws.Cells[startRow, 6] = b.CartonQuantity;
                //_ws.Cells[startRow, 7] = b.IsMainItem ? b.PalletQuantity.ToString() : " ";
                //_ws.Cells[startRow, 8] = b.Location;

                if (b.IsMainItem)
                {
                    mergeRange = _ws.get_Range("G" + mergeStartRow, "G" + mergeEndRow);
                    mergeRange.Merge(mergeRange.MergeCells);
                    _ws.Cells[startRow, 7] = b.ActualPallets.ToString();
                    mergeStartRow = startRow;
                    mergeEndRow = startRow;
                }
                else
                {
                    mergeEndRow += 1;
                }

                startRow += 1;
            }

            mergeRange = _ws.get_Range("G" + mergeStartRow, "G" + mergeEndRow);
            mergeRange.Merge(mergeRange.MergeCells);

            var lastRow = startRow + 2;

            if (lastRow < 37)
            {
                lastRow = 37;
            }

            _ws.Cells[lastRow, 1] = "Total";
            _ws.Cells[lastRow, 6] = bolDetailList.Sum(x => x.CartonQuantity);
            _ws.Cells[lastRow, 7] = bolDetailList.Sum(x => x.ActualPallets);

            if (orderType == FBAOrderType.MasterOrder)
            {
                _ws.Cells[lastRow, 7] = bolDetail.PltQuantity;
                _ws.Cells[21, 7] = bolDetail.PltQuantity;
            }

            //for(int i = 21; i <= lastRow; i++)
            //{
            //    for(int j = 1; j <= 7; j++)
            //    {
            //        _ws.Cells[i, j].HorizontalAlignment = XlHAlign.xlHAlignCenter;
            //        _ws.Cells[i, j].HorizontalAlignment = XlVAlign.xlVAlignCenter;
            //    }
            //}

            var range = _ws.get_Range("A20", "G50");
            range.HorizontalAlignment = XlHAlign.xlHAlignCenter;
            range.HorizontalAlignment = XlVAlign.xlVAlignCenter;
            range.VerticalAlignment = XlHAlign.xlHAlignCenter;
            range.VerticalAlignment = XlVAlign.xlVAlignCenter;

            range.WrapText = true;

            var fullPath = @"E:\BOL\FBA-BOL-" + orderNumber + "-" + DateTime.Now.ToString("yyyyMMddhhmmssffff") + ".xlsx";
            _wb.SaveAs(fullPath, Type.Missing, "", "", Type.Missing, Type.Missing, XlSaveAsAccessMode.xlNoChange, 1, false, Type.Missing, Type.Missing, Type.Missing);

            _excel.Quit();

            return fullPath;
        }

        //生成Excel版本的出库计划报告并返回完整路径
        public string GenerateWarehouseSchedule(string customerCode, DateTime fromDate, DateTime toDate, IList<WarehouseOutboundLog> outboundList, IList<WarehouseInboundLog> inboundList)
        {
            //填充入库报告
            _ws = _wb.Worksheets[1];

            _ws.Cells[4, 2] = fromDate.ToString("yyyy/MM/dd");
            _ws.Cells[4, 4] = toDate.ToString("yyyy/MM/dd");
            _ws.Cells[4, 19] = DateTime.Now.ToString("yyyy/MM/dd");
            _ws.Cells[6, 2] = inboundList.Count;
            _ws.Cells[5, 19] = inboundList.Sum(x => x.Ctns);
            _ws.Cells[6, 4] = inboundList.Sum(x => x.SKU);
            _ws.Cells[6, 19] = inboundList.Sum(x => x.OriginalPlts);

            var startIndex = 9;

            foreach (var l in inboundList)
            {
                _ws.Cells[startIndex, 1] = l.Status;
                _ws.Cells[startIndex, 2] = l.UnloadingType == "FCL" ? "FCL(" + l.ContainerSize + ")" : l.UnloadingType;
                _ws.Cells[startIndex, 3] = l.CustomerCode;
                _ws.Cells[startIndex, 4] = l.SubCustomer;
                _ws.Cells[startIndex, 5] = l.ETA;
                _ws.Cells[startIndex, 6] = l.InboundDate.Year == 1900 ? "" : l.InboundDate.ToString("yyyy-MM-dd");
                _ws.Cells[startIndex, 7] = l.DockNumber;
                _ws.Cells[startIndex, 8] = l.Container;
                _ws.Cells[startIndex, 9] = l.SKU;
                _ws.Cells[startIndex, 10] = l.OriginalCtns;
                _ws.Cells[startIndex, 11] = l.OriginalPlts;
                _ws.Cells[startIndex, 12] = l.Carrier;
                _ws.Cells[startIndex, 13] = l.Lumper;
                _ws.Cells[startIndex, 14] = l.PushTime.Year == 1900 ? "" : l.PushTime.ToString("yyyy-MM-dd");
                _ws.Cells[startIndex, 15] = l.UnloadStartTime.Year == 1900 ? "" : l.UnloadStartTime.ToString("yyyy-MM-dd");
                _ws.Cells[startIndex, 16] = l.UnloadFinishTime.Year == 1900 ? "" : l.UnloadFinishTime.ToString("yyyy-MM-dd");
                _ws.Cells[startIndex, 17] = l.AvailableTime.Year == 1900 ? "" : l.AvailableTime.ToString("yyyy-MM-dd");
                _ws.Cells[startIndex, 18] = l.OutTime.Year == 1900 ? "" : l.OutTime.ToString("yyyy-MM-dd");
                _ws.Cells[startIndex, 19] = l.Instruction;

                startIndex++;
            }

            var range = _ws.get_Range("A1", "S" + startIndex);
            range.HorizontalAlignment = XlHAlign.xlHAlignCenter;
            range.HorizontalAlignment = XlVAlign.xlVAlignCenter;
            range.VerticalAlignment = XlHAlign.xlHAlignCenter;
            range.VerticalAlignment = XlVAlign.xlVAlignCenter;
            range.Borders.LineStyle = 1;
            range.WrapText = true;

            //填充出库报告
            _ws = _wb.Worksheets[2];

            _ws.Cells[4, 2] = fromDate.ToString("yyyy/MM/dd");
            _ws.Cells[4, 4] = toDate.ToString("yyyy/MM/dd");
            _ws.Cells[4, 12] = DateTime.Now.ToString("yyyy/MM/dd");
            _ws.Cells[6, 2] = outboundList.Count;
            _ws.Cells[5, 12] = outboundList.Sum(x => x.TotalCtns);
            _ws.Cells[6, 12] = outboundList.Sum(x => x.TotalPlts);

            startIndex = 9;

            foreach(var l in outboundList)
            {
                _ws.Cells[startIndex, 1] = l.Status;
                _ws.Cells[startIndex, 2] = l.Department;
                _ws.Cells[startIndex, 3] = l.CustomerCode;
                _ws.Cells[startIndex, 4] = l.SubCustomer;
                _ws.Cells[startIndex, 5] = l.OrderNumber;
                _ws.Cells[startIndex, 6] = l.Destination;
                _ws.Cells[startIndex, 7] = l.Carrier;
                _ws.Cells[startIndex, 8] = l.TotalCtns;
                _ws.Cells[startIndex, 9] = l.TotalPlts;
                _ws.Cells[startIndex, 10] = l.ETS + " " + l.ETSTimeRange;
                _ws.Cells[startIndex, 11] = l.PlaceTime.Year == 1900 ? "-" : l.PlaceTime.ToString("yyyy-MM-dd hh:mm");
                _ws.Cells[startIndex, 12] = l.ReadyTime.Year == 1900 ? "-" : l.ReadyTime.ToString("yyyy-MM-dd hh:mm");
                startIndex++;
            }

            range = _ws.get_Range("A1", "L" + startIndex);
            range.HorizontalAlignment = XlHAlign.xlHAlignCenter;
            range.HorizontalAlignment = XlVAlign.xlVAlignCenter;
            range.VerticalAlignment = XlHAlign.xlHAlignCenter;
            range.VerticalAlignment = XlVAlign.xlVAlignCenter;
            range.Borders.LineStyle = 1;
            range.WrapText = true;

            var fullPath = @"E:\BOL\" + customerCode + "_" + fromDate.ToString("MMddyyyy") + "_" + toDate.ToString("MMddyyyy") + "-WarehouseSchedule-" + DateTime.Now.ToString("yyyyMMddhhmmssffff") + ".xlsx";
            _wb.SaveAs(fullPath, Type.Missing, "", "", Type.Missing, Type.Missing, XlSaveAsAccessMode.xlNoChange, 1, false, Type.Missing, Type.Missing, Type.Missing);

            _excel.Quit();

            return fullPath;
        }
 
        private IList<FBABOLDetail> GenerateFBABOLList(IEnumerable<FBAPickDetail> pickDetailsInDb)
        {
            var bolList = new List<FBABOLDetail>();

            foreach (var pickDetail in pickDetailsInDb)
            {
                if (pickDetail.FBAPalletLocation != null)
                {
                    var cartonInPickList = pickDetail.FBAPickDetailCartons.ToList();
                    for (int i = 0; i < cartonInPickList.Count; i++)
                    {
                        //var plt = pickDetail.ActualPlts;

                        bolList.Add(new FBABOLDetail
                        {
                            ParentPalletId = pickDetail.FBAPalletLocation.Id,
                            CustomerOrderNumber = cartonInPickList[i].FBACartonLocation.ShipmentId,
                            Contianer = pickDetail.Container,
                            AmzRef = pickDetail.AmzRefId,
                            CartonQuantity = cartonInPickList[i].PickCtns,
                            ActualPallets = pickDetail.ActualPlts,
                            PickPallets = pickDetail.PltsFromInventory,
                            Weight = cartonInPickList[i].FBACartonLocation.GrossWeightPerCtn * cartonInPickList[i].PickCtns,
                            Location = pickDetail.Location
                        });
                    }
                }
                else
                {
                    bolList.Add(new FBABOLDetail
                    {
                        ParentPalletId = 0,
                        CustomerOrderNumber = pickDetail.ShipmentId,
                        Contianer = pickDetail.Container,
                        CartonQuantity = pickDetail.ActualQuantity,
                        AmzRef = pickDetail.AmzRefId,
                        ActualPallets = 0,
                        PickPallets = 0,
                        Weight = pickDetail.ActualGrossWeight,
                        Location = pickDetail.Location
                    });
                }
            }

            return bolList;
        }
    }

    public class ShipRecord
    {
        public string Reference { get; set; }

        public string WarehouseLocation { get; set; }

        public int ShippedPlts { get; set; }

        public string InboundDate { get; set; }

        public string OutboundDate { get; set; }

        public string PalletSize { get; set; }

        public bool IsPreReleased { get; set; }

        public string subCustomer { get; set; }

        public int ShippedCtns { get; set; }

        public ShipRecord()
        {
            IsPreReleased = false;
        }
    }
}