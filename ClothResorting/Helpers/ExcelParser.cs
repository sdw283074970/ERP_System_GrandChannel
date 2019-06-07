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

            foreach(var po in poList)
            {
                var startIndex = po.Id;
                po.Id = 0;

                //TO DO
            }

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
                    throw new Exception("Error detected. Please check row " + startIndex);
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
                var emptyCount = 5;

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
                            Vendor = Vendor.FreeCountry
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

            return poList;
        }
    }
}