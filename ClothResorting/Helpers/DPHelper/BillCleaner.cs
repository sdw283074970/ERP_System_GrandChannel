using Microsoft.Office.Interop.Excel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;

namespace ClothResorting.Helpers.DPHelper
{
    public class BillCleaner
    {
        //全局变量
        #region
        private _Application _excel;
        private Workbook _wb;
        private Worksheet _ws;
        private DateTime _dateTimeNow;
        private string _userName;
        #endregion

        //构造器
        public BillCleaner()
        {
            _dateTimeNow = DateTime.Now;
            _userName = HttpContext.Current.User.Identity.Name.Split('@')[0];
        }

        //清理账单，将收费单号和收费项目、价格分离
        public string ClearBills(string path)
        {
            _excel = new Application();
            _wb = _excel.Workbooks.Open(path);
            _ws = _wb.Worksheets[1];
            var billCount = -1;
            var index = 1;
            var billList = new List<LabelBill>();
            var chargingItemList = new List<string>();

            while(_ws.Cells[index, 1].Value2 != null)
            {
                billCount++;
                index++;
            }

            _ws.Cells[1, 35] = "Length";
            _ws.Cells[1, 36] = "Width";
            _ws.Cells[1, 37] = "Height";

            for (int i = 0; i < billCount; i++)
            {
                var newBill = new LabelBill();
                var str = _ws.Cells[i + 2, 35].Value2.ToString();

                newBill.LabelCode = _ws.Cells[i + 2, 1].Value2.ToString();
                newBill.LabelBillDetails = ConvertStringToDetail(str, chargingItemList);
                billList.Add(newBill);

                string dms = _ws.Cells[i + 2, 34].Value2 == null ? "0x0x0" : _ws.Cells[i + 2, 34].Value2.ToString();

                if (dms == "0")
                {
                    _ws.Cells[i + 2, 35] = 0;
                    _ws.Cells[i + 2, 36] = 0;
                    _ws.Cells[i + 2, 37] = 0;
                    _ws.Cells[i + 2, 37].NumberFormat = "#,###,###";
                }
                else
                {
                    _ws.Cells[i + 2, 35] = dms.Split('x')[0];
                    _ws.Cells[i + 2, 36] = dms.Split('x')[1];
                    _ws.Cells[i + 2, 37] = dms.Split('x')[2];
                    _ws.Cells[i + 2, 37].NumberFormat = "#,###,###";
                }
            }

            index++;

            index = 1;

            var itemArray = chargingItemList.ToArray();
            //_ws.Cells[index, 1] = "Label Code";

            for (int i = 0; i < itemArray.Length; i++)
            {
                _ws.Cells[index, i + 38] = itemArray[i];
            }

            index++;

            foreach(var b in billList)
            {
                //_ws.Cells[index, 1] = b.LabelCode;

                foreach(var l in b.LabelBillDetails)
                {
                    var itemIndex = Array.IndexOf(itemArray, l.ChargingItem);
                    _ws.Cells[index, itemIndex + 38] = l.Rate;
                }

                index++;
            }

            _wb.Save();

            //var fullPath = @"D:\PickingList\test.xlsx";
            //_wb.SaveAs(fullPath, Type.Missing, "", "", Type.Missing, Type.Missing, XlSaveAsAccessMode.xlNoChange, 1, false, Type.Missing, Type.Missing, Type.Missing);
            _wb.Close();
            _excel.Quit();

            var killer = new ExcelKiller();

            killer.Dispose();

            return path;
        }

        private IList<LabelBillDetail> ConvertStringToDetail(string str, List<string> chargingItemList)
        {
            var list = new List<LabelBillDetail>();
            var details = str.Split(';');

            foreach(var d in details)
            {
                string cleanedDetails = Regex.Replace(d, @"\t|\n|\r", "");

                if (cleanedDetails == " " || cleanedDetails == "")
                {
                    continue;
                }

                var item = cleanedDetails.Split(':')[0].ToString();
                var rate = cleanedDetails.Split(':')[1];

                if (!chargingItemList.Contains(item))
                {
                    chargingItemList.Add(item);
                }

                var newDetail = new LabelBillDetail
                {
                    ChargingItem = item,
                    Rate = rate
                };

                list.Add(newDetail);
            }

            return list;
        }
    }

    public class LabelBill
    {
        public string LabelCode { get; set; }

        public List<LabelBillDetail> LabelBillDetails { get; set; }
    }

    public class LabelBillDetail
    {
        public string ChargingItem { get; set; }

        public string Rate { get; set; }
    }
}