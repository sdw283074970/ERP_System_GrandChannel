﻿using ClothResorting.Models;
using Microsoft.Office.Interop.Excel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Entity;
using ClothResorting.Models.StaticClass;
using ClothResorting.Models.FBAModels.StaticModels;
using ClothResorting.Models.FBAModels;

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

            var unPalletizedSKU = masterOrderInDb.FBAOrderDetails
                .Where(x => x.ComsumedQuantity < x.ActualQuantity);

            var palletsInDb = _context.FBAPallets
                .Include(x => x.FBAMasterOrder)
                .Include(x => x.FBACartonLocations)
                .Where(x => x.FBAMasterOrder.Id == masterOrderId);

            var totalPlts = palletsInDb.Sum(x => x.ActualPallets);

            _ws = _wb.Worksheets[1];
            var startRow = 5;

            //foreach(var d in masterOrderInDb.FBAOrderDetails)
            //{
            //    _ws.Cells[startRow, 1] = d.ShipmentId;
            //    _ws.Cells[startRow, 2] = d.AmzRefId;
            //    _ws.Cells[startRow, 3] = d.ActualCBM;
            //    _ws.Cells[startRow, 4] = d.ActualQuantity;

            //    startRow += 1;
            //}

            var ws = (Worksheet)_wb.ActiveSheet;

            //写入打托的SKU
            foreach (var p in palletsInDb)
            {
                //合并单元格
                _ws.Cells[startRow, 5] = p.ActualPallets;
                Range c1 = _ws.Cells[startRow, 5];
                Range c2 = _ws.Cells[startRow + p.FBACartonLocations.Count - 1, 5];
                Range range = ws.get_Range(c1, c2);
                //range.EntireColumn.AutoFit();

                foreach (var c in p.FBACartonLocations)
                {
                    _ws.Cells[startRow, 1] = c.ShipmentId;
                    _ws.Cells[startRow, 2] = c.AmzRefId;
                    _ws.Cells[startRow, 3] = c.FBAOrderDetail.Quantity;
                    _ws.Cells[startRow, 4] = c.ActualQuantity;
                    startRow += 1;
                }

            }

            //写入未打托的SKU
            foreach(var s in unPalletizedSKU)
            {
                _ws.Cells[startRow, 1] = s.ShipmentId;
                _ws.Cells[startRow, 2] = s.AmzRefId;
                _ws.Cells[startRow, 3] = s.Quantity;
                _ws.Cells[startRow, 4] = s.ActualQuantity;
                startRow += 1;
            }

            //写入表头信息
            _ws.Cells[2, 2] = masterOrderInDb.Customer.CustomerCode;
            _ws.Cells[2, 4] = masterOrderInDb.InboundDate.ToString("yyyy-MM-dd");
            _ws.Cells[3, 2] = masterOrderInDb.Container;
            _ws.Cells[3, 4] = masterOrderInDb.OriginalPlts;

            //_ws.Cells[startRow, 1] = "Total Plts:";
            //_ws.Cells[startRow, 2] = totalPlts;
            //_ws.Cells[startRow, 3] = "Total Ctns";
            //_ws.Cells[startRow, 4] = masterOrderInDb.FBAOrderDetails.Sum(x => x.ActualQuantity);

            //写入表脚信息
            _ws.Cells[startRow, 1] = "Total:";
            _ws.Cells[startRow, 3] = masterOrderInDb.FBAOrderDetails.Sum(x => x.Quantity);
            _ws.Cells[startRow, 4] = masterOrderInDb.FBAOrderDetails.Sum(x => x.ActualQuantity);
            _ws.Cells[startRow, 5] = palletsInDb.Sum(x => x.ActualPallets);

            var fullPath = @"D:\Receipts\FBA-" + masterOrderInDb.Customer.CustomerCode + "-Receipt-" + DateTime.Now.ToString("yyyyMMddhhmmssffff") + ".xlsx";
            _wb.SaveAs(fullPath, Type.Missing, "", "", Type.Missing, Type.Missing, XlSaveAsAccessMode.xlNoChange, 1, false, Type.Missing, Type.Missing, Type.Missing);

            _excel.Quit();

            return fullPath;
        }

        //生成StorageFee报告并返回完整路径
        public string GenerateStorageReport(int customerId, DateTime startDate, DateTime closeDate)
        {
            var actualCloseDate = closeDate.AddDays(1);

            var customerInDb = _context.UpperVendors.Find(customerId);

            var palletLocationInDb = _context.FBAPalletLocations
                .Include(x => x.FBAMasterOrder.Customer)
                .Include(x => x.FBAMasterOrder.FBAPallets)
                .Include(x => x.FBAPickDetails.Select(c => c.FBAShipOrder))
                .Where(x => x.FBAMasterOrder.InboundDate < actualCloseDate
                    && x.FBAMasterOrder.Customer.Id == customerId);

            var pickDetailInDb = _context.FBAPickDetails
                .Include(x => x.FBAPalletLocation.FBAMasterOrder.Customer)
                .Include(x => x.FBAShipOrder)
                .Where(x => x.FBAShipOrder.ShipDate < actualCloseDate
                    && x.FBAShipOrder.ShipDate >= startDate
                    && x.FBAShipOrder.Status == FBAStatus.Shipped
                    && x.FBAPalletLocation != null
                    && x.FBAPalletLocation.FBAMasterOrder.InboundDate < actualCloseDate
                    && x.FBAPalletLocation.FBAMasterOrder.Customer.Id == customerId
                    && x.PltsFromInventory != 0);

            //var cartonLocationInDb = _context.FBACartonLocations
            //    .Include(x => x.FBAOrderDetail.FBAMasterOrder.Customer)
            //    .Where(x => x.FBAOrderDetail.FBAMasterOrder.InboundDate <= closeDate 
            //        && x.FBAOrderDetail.FBAMasterOrder.Customer.Id == customerId);

            foreach (var p in palletLocationInDb)
            {
                foreach(var pick in p.FBAPickDetails)
                {
                    //从原有托盘数量扣除状态为shipped状态，且发货日期在结束日期之前的运单中的托盘数量
                    if(pick.FBAShipOrder.Status == FBAStatus.Shipped && pick.FBAShipOrder.ShipDate < actualCloseDate)
                        p.ActualPlts -= pick.PltsFromInventory;
                }
            }

            _ws = _wb.Worksheets[1];
            var startIndex = 2;

            var palletsInDbGroup = palletLocationInDb.GroupBy(x => x.Container);

            //对仓库剩余托盘进行收费
            foreach(var p in palletsInDbGroup)
            {
                //var pallets = p.Sum(x => x.ActualPlts);
                var standardPlts = p.Where(x => x.PalletSize == "P1").Sum(x => x.ActualPlts);
                var plusPlts = p.Where(x => x.PalletSize == "P2").Sum(x => x.ActualPlts);
                var pallets = standardPlts + 2 * plusPlts;
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
                    PalletSize = s.Size
                };

                var sameShipRecord = shipList.SingleOrDefault(x => x.Reference == newShipRecord.Reference
                    && x.InboundDate == newShipRecord.InboundDate
                    && x.OutboundDate == newShipRecord.OutboundDate);

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
                    _ws.Cells[startIndex, 7] = s.ShippedPlts;
                }
                else if (s.PalletSize == "P2")
                {
                    _ws.Cells[startIndex, 6] = s.ShippedPlts;
                    _ws.Cells[startIndex, 7] = s.ShippedPlts * 2;
                }
                else
                {
                    _ws.Cells[startIndex, 7] = 0;
                }

                _ws.Cells[startIndex, 1] = FBAOrderType.ShipOrder;
                _ws.Cells[startIndex, 2] = s.Reference;
                _ws.Cells[startIndex, 8] = s.InboundDate;
                _ws.Cells[startIndex, 9] = s.OutboundDate;

                startIndex += 1;
            }

            var fullPath = @"D:\StorageFee\FBA-" + customerInDb.CustomerCode + "-StorageFee-" + DateTime.Now.ToString("yyyyMMddhhmmssffff") + ".xlsx";
            _wb.SaveAs(fullPath, Type.Missing, "", "", Type.Missing, Type.Missing, XlSaveAsAccessMode.xlNoChange, 1, false, Type.Missing, Type.Missing, Type.Missing);

            _excel.Quit();

            return fullPath;
        }

        //生成拣货单并返回完整路径
        public string GeneratePickingList(int shipOrderId)
        {
            _ws = _wb.Worksheets[1];
            var startIndex = 3;

            var pickDetailInDb = _context.FBAPickDetails
                .Include(x => x.FBAShipOrder)
                .Include(x => x.FBAPickDetailCartons)
                .Include(x => x.FBAPalletLocation.FBAPallet.FBACartonLocations)
                .Where(x => x.FBAShipOrder.Id == shipOrderId)
                .ToList();

            _ws.Cells[1, 2] = pickDetailInDb.First().FBAShipOrder.ShipOrderNumber.ToString();
            var bolList = GenerateFBABOLList(pickDetailInDb);
            foreach(var p in bolList)
            {
                _ws.Cells[startIndex, 1] = p.CustomerOrderNumber;
                _ws.Cells[startIndex, 2] = p.Contianer;
                _ws.Cells[startIndex, 3] = p.CartonQuantity;
                _ws.Cells[startIndex, 5] = p.Location;

                if (p.PalletQuantity != 0)
                {
                    _ws.Cells[startIndex, 4] = p.PalletQuantity;
                }

                startIndex += 1;
            }

            _ws.Cells[startIndex + 1, 2] = "Total";
            _ws.Cells[startIndex + 1, 3] = pickDetailInDb.Sum(x => x.ActualQuantity);
            _ws.Cells[startIndex + 1, 4] = pickDetailInDb.Sum(x => x.ActualPlts);

            var fullPath = @"D:\PickingList\FBA-" + pickDetailInDb.First().FBAShipOrder.CustomerCode + "-PickingList-" + DateTime.Now.ToString("yyyyMMddhhmmssffff") + ".xlsx";
            _wb.SaveAs(fullPath, Type.Missing, "", "", Type.Missing, Type.Missing, XlSaveAsAccessMode.xlNoChange, 1, false, Type.Missing, Type.Missing, Type.Missing);

            _excel.Quit();

            return fullPath;
        }

        //生成Excel版本的BOL并返回完整路径
        public string GenerateExcelBol(int shipOrderId, IList<FBABOLDetail> bolDetailList)
        {
            var shipOrderInDb = _context.FBAShipOrders.Find(shipOrderId);
            var addressBookInDb = _context.FBAAddressBooks.SingleOrDefault(x => x.WarehouseCode == shipOrderInDb.Destination);
            var address = " ";
            _ws = _wb.Worksheets[1];

            if (addressBookInDb != null)
            {
                address = addressBookInDb.Address;
            }

            //设置BOL时间
            _ws.Cells[2, 1] = "Date: " + DateTime.Now.ToString("yyyy-MM-dd");

            //设置BOL#
            _ws.Cells[3, 6] = shipOrderInDb.BOLNumber;

            //设置地址
            _ws.Cells[7, 2] = shipOrderInDb.Destination;
            _ws.Cells[8, 1] = address;

            //设置carrier
            _ws.Cells[6, 6] = shipOrderInDb.Carrier;

            //设置Ship Order #
            _ws.Cells[18, 1] = "Ship Order#: " + shipOrderInDb.ShipOrderNumber;

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
                _ws.Cells[startRow, 5] = b.Weight;
                _ws.Cells[startRow, 6] = b.CartonQuantity;
                //_ws.Cells[startRow, 7] = b.IsMainItem ? b.PalletQuantity.ToString() : " ";
                //_ws.Cells[startRow, 8] = b.Location;

                if (b.IsMainItem)
                {
                    mergeRange = _ws.get_Range("G" + mergeStartRow, "G" + mergeEndRow);
                    mergeRange.Merge(mergeRange.MergeCells);
                    _ws.Cells[startRow, 7] = b.PalletQuantity.ToString();
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
            _ws.Cells[lastRow, 7] = bolDetailList.Sum(x => x.PalletQuantity);

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

            var fullPath = @"D:\BOL\FBA-BOL-" + shipOrderInDb.ShipOrderNumber + "-" + DateTime.Now.ToString("yyyyMMddhhmmssffff") + ".xlsx";
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
                        var plt = pickDetail.ActualPlts;

                        bolList.Add(new FBABOLDetail
                        {
                            CustomerOrderNumber = cartonInPickList[i].FBACartonLocation.ShipmentId,
                            Contianer = pickDetail.Container,
                            CartonQuantity = cartonInPickList[i].PickCtns,
                            PalletQuantity = plt,
                            Weight = cartonInPickList[i].FBACartonLocation.GrossWeightPerCtn * cartonInPickList[i].PickCtns,
                            Location = pickDetail.Location
                        });
                    }
                }
                else
                {
                    bolList.Add(new FBABOLDetail
                    {
                        CustomerOrderNumber = pickDetail.ShipmentId,
                        Contianer = pickDetail.Container,
                        CartonQuantity = pickDetail.ActualQuantity,
                        PalletQuantity = 0,
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

        public int ShippedPlts { get; set; }

        public string InboundDate { get; set; }

        public string OutboundDate { get; set; }

        public string PalletSize { get; set; }
    }
}