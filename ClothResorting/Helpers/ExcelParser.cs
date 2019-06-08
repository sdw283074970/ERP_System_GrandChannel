using ClothResorting.Models;
using ClothResorting.Models.StaticClass;
using Microsoft.Office.Interop.Excel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;

namespace ClothResorting.Helpers
{
    public class ExcelParser
    {
        //全局变量
        #region
        private ApplicationDbContext _context;
        private _Application _excel;
        private Workbook _wb;
        private Worksheet _ws;
        private string _userName;
        private Range range = null;
        #endregion

        public ExcelParser(string path)
        {
            _context = new ApplicationDbContext();
            _excel = new Application();
            _wb = _excel.Workbooks.Open(path);
            _userName = HttpContext.Current.User.Identity.Name.Split('@')[0];
        }

        //解析FreeCountry的packing List（不需要summary的版本）
        public void ParseFreeCountryPackingListV2(int preId)
        {
            var preReceiveOrderInDb = _context.PreReceiveOrders.Find(preId);
            _ws = _wb.Worksheets[2];
            //首先检查文件是否符合FC模板格式，并计算PO summary的数量
            var poList = CheckFCTemplateAndReturnPOSummaryIndexList(preReceiveOrderInDb);

            _context.POSummaries.AddRange(poList);

            var cartonList = new List<RegularCartonDetail>();

            foreach(var po in poList)
            {
                var startIndex = po.Id;
                po.Id = 0;

                //获取size的数量
                var endColumnIndex = 12;

                while(_ws.Cells[startIndex + 2, endColumnIndex].Value2 != "Pack")
                {
                    endColumnIndex += 1;
                }

                //获取SKU的数量
                var endRowIndex = startIndex + 3;

                while (_ws.Cells[endRowIndex, 1].Value2 != "Totals")
                {
                    endRowIndex += 1;
                }

                endColumnIndex -= 1;
                endRowIndex -= 1;

                for(int i = startIndex + 3; i <= endRowIndex; i++)
                {
                    var cartonRange = _ws.Cells[i, 1].Value2.ToString();
                    var purchaseOrder = _ws.Cells[i, 2].Value2.ToString();
                    var style = _ws.Cells[i, 3].Value2.ToString();
                    var customer = _ws.Cells[i, 4].Value2.ToString();
                    var colorCode = _ws.Cells[i, 9].Value2.ToString();
                    var color = _ws.Cells[i, 10].Value2.ToString();
                    var cartons = (int)_ws.Cells[i, 11].Value2;

                    for (int j = 12; j <= endColumnIndex; j++)
                    {
                        var size = _ws.Cells[startIndex + 2, j].Value2.ToString();
                        var pcs = (int)_ws.Cells[i, j].Value2;

                        var newCartnDetail = new RegularCartonDetail {
                            CartonRange = cartonRange,
                            PurchaseOrder = purchaseOrder,
                            Style = style,
                            Customer = customer,
                            Color = color,
                            ColorCode = colorCode,
                            Cartons = j == 12 ? cartons : 0,
                            PcsBundle = pcs.ToString(),
                            PcsPerCarton = pcs,
                            Quantity = pcs * cartons,
                            POSummary = po,
                            Comment = "",
                            OrderType = OrderType.SolidPack,
                            Operator = _userName,
                            Adjustor = "",
                            Receiver = "",
                            Batch = po.Batch,
                            Vendor = Vendor.FreeCountry,
                            SizeBundle = size,
                            Status = Status.NewCreated
                        };

                        cartonList.Add(newCartnDetail);
                    }
                }
            }

            _context.RegularCartonDetails.AddRange(cartonList);
            _context.SaveChanges();

            _wb.Save();

            var killer = new ExcelKiller();

            killer.Dispose();
        }

        private IEnumerable<POSummary> CheckFCTemplateAndReturnPOSummaryIndexList(PreReceiveOrder preReceiveOrderInDb)
        {
            var poList = new List<POSummary>();
            var index = 1;
            var startIndex = 1;
            var isEnd = false;
            var batch = preReceiveOrderInDb.LastBatch + 1;

            //进行检查，检测出以下情况报错：
            //1.对象第1行第1列不是“Order”
            //2.对象第3行第3列不是“Style#”
            //3.对象第3行第11列不是“Cartons”
            //4.对象最后一行第1列不是“Totals”
            //5.检测到Totals但不是最后一行

            while (index > 0)
            {
                if (_ws.Cells[startIndex, 1].Value2 != "Order")
                {
                    throw new Exception("PO head missing. Please check row " + startIndex);
                }

                if (_ws.Cells[startIndex + 2, 3].Value2 != "Style #")
                {
                    throw new Exception("Style # column does not match the template. PLease check cell [" + (startIndex + 2) + ",3]");
                }

                if (_ws.Cells[startIndex + 2, 11].Value2 != "Cartons")
                {
                    throw new Exception("Column does not match the template. PLease check row " + (startIndex + 2) + " and make sure the column 'Cartons' is the 11th column");
                }

                var currentValue = _ws.Cells[index, 1].Value2;
                var emptyCount = 6;

                if (currentValue != null)
                {
                    index += 1;

                    if (currentValue == "Totals")
                    {
                        if (_ws.Cells[index, 1].Value2 != null)
                        {
                            throw new Exception("A spece row is required between tow objects. Please check row " + index);
                        }

                        poList.Add(new POSummary
                        {
                            Id = startIndex,
                            PurchaseOrder = _ws.Cells[startIndex + 1, 1].Value2.ToString(),
                            Style = _ws.Cells[startIndex + 1, 2].Value2.ToString(),
                            PoLine = (int)_ws.Cells[startIndex + 1, 3].Value2,
                            Customer = _ws.Cells[startIndex + 3, 4].Value2.ToString(),
                            Container = Status.Unknown,
                            PreReceiveOrder = preReceiveOrderInDb,
                            Operator = _userName,
                            Batch = batch.ToString(),
                            Vendor = Vendor.FreeCountry,
                            OrderType = OrderType.Regular
                        });

                        batch += 1;
                    }
                }
                else
                {
                    if (_ws.Cells[index - 1, 1].Value2 != "Totals")
                    {
                        throw new Exception("Every object must end with 'Totals'. Please check row " + index);
                    }

                    index += 1;

                    while (emptyCount > 0)
                    {
                        if (emptyCount == 1)
                        {
                            isEnd = true;
                        }

                        if (_ws.Cells[index, 1].Value2 == null)
                        {
                            range = (Range)_ws.Rows[index, Missing.Value];
                            range.Delete(XlDirection.xlDown);
                            emptyCount -= 1;
                        }
                        else
                        {
                            startIndex = index;
                            break;
                        }
                    }
                }

                if (isEnd)
                    break;
            }

            preReceiveOrderInDb.LastBatch = batch - 1;

            return poList;
        }
    }
}