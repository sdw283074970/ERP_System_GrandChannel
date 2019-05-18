using Microsoft.Office.Interop.Excel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ClothResorting.Helpers.DPHelper
{
    public class BillCleaner
    {
        //全局变量
        #region
        private string _path = "";
        private _Application _excel;
        private Workbook _wb;
        private Worksheet _ws;
        private DateTime _dateTimeNow;
        private string _userName;
        #endregion

        //构造器
        public BillCleaner(string path)
        {
            _path = path;
            _dateTimeNow = DateTime.Now;
            _excel = new Application();
            _wb = _excel.Workbooks.Open(_path);
            _userName = HttpContext.Current.User.Identity.Name.Split('@')[0];
        }

        //清理账单，将收费单号和收费项目、价格分离
        public string ClearBills()
        {
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

            for (int i = 0; i < billCount; i++)
            {
                var newBill = new LabelBill();
                var str = _ws.Cells[i + 2, 35].Value2.ToString();

                newBill.LabelCode = _ws.Cells[i + 2, 1].Value2.ToString();
                newBill.LabelBillDetails = ConvertStringToDetail(str, chargingItemList);
                billList.Add(newBill);
            }

            index++;

            var itemArray = chargingItemList.ToArray();
            _ws.Cells[index, 1] = "Label Code";

            for (int i = 0; i < itemArray.Length; i++)
            {
                _ws.Cells[index, i + 2] = itemArray[i];
            }

            index++;

            foreach(var b in billList)
            {
                _ws.Cells[index, 1] = b.LabelCode;

                foreach(var l in b.LabelBillDetails)
                {
                    var itemIndex = Array.IndexOf(itemArray, l.ChargingItem);
                    _ws.Cells[index, itemIndex + 2] = l.Rate;
                }

                index++;
            }

            _wb.Save();
            //var fullPath = @"D:\PickingList\test.xlsx";
            //_wb.SaveAs(fullPath, Type.Missing, "", "", Type.Missing, Type.Missing, XlSaveAsAccessMode.xlNoChange, 1, false, Type.Missing, Type.Missing, Type.Missing);

            _excel.Quit();

            return _path;
        }

        private IList<LabelBillDetail> ConvertStringToDetail(string str, List<string> chargingItemList)
        {
            var list = new List<LabelBillDetail>();
            var details = str.Split(';');

            foreach(var d in details)
            {
                if (d == " ")
                {
                    continue;
                }

                var item = d.Split(':')[0].ToString();
                var rate = d.Split(':')[1];

                if (item.Contains("\n"))
                {
                    item = item.Substring(2);
                }

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