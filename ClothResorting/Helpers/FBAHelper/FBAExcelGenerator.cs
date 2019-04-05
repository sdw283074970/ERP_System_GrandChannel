using ClothResorting.Models;
using Microsoft.Office.Interop.Excel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Entity;

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

        //生成Receipt文件完整路径
        public string GenerateReceipt(int masterOrderId)
        {
            var masterOrderInDb = _context.FBAMasterOrders
                .Include(x => x.FBAOrderDetails)
                .Include(x => x.Customer)
                .SingleOrDefault(x => x.Id == masterOrderId);

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

            _ws.Cells[2, 2] = masterOrderInDb.Customer.CustomerCode;
            _ws.Cells[2, 4] = masterOrderInDb.InboundDate.ToString("yyyy-MM-dd");
            _ws.Cells[3, 2] = masterOrderInDb.Container;
            _ws.Cells[3, 4] = masterOrderInDb.OriginalPlts;

            //_ws.Cells[startRow, 1] = "Total Plts:";
            //_ws.Cells[startRow, 2] = totalPlts;
            //_ws.Cells[startRow, 3] = "Total Ctns";
            //_ws.Cells[startRow, 4] = masterOrderInDb.FBAOrderDetails.Sum(x => x.ActualQuantity);

            _ws.Cells[startRow, 1] = "Total:";
            _ws.Cells[startRow, 3] = masterOrderInDb.FBAOrderDetails.Sum(x => x.Quantity);
            _ws.Cells[startRow, 4] = masterOrderInDb.FBAOrderDetails.Sum(x => x.ActualQuantity);
            _ws.Cells[startRow, 5] = palletsInDb.Sum(x => x.ActualPallets);

            var fullPath = @"D:\Receipts\FBA-" + masterOrderInDb.Customer.CustomerCode + "-Receipt-" + DateTime.Now.ToString("yyyyMMddhhmmssffff") + ".xlsx";
            _wb.SaveAs(fullPath, Type.Missing, "", "", Type.Missing, Type.Missing, XlSaveAsAccessMode.xlNoChange, 1, false, Type.Missing, Type.Missing, Type.Missing);

            _excel.Quit();

            return fullPath;
        }
    }
}